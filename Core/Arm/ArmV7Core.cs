using System;
using System.Collections.Generic;

namespace ProcessorEmulator.Arm
{
    // ARMV7_A15 skeleton (not wired). See docs/ARMV7_A15_DESIGN.md.
    public interface IArmBus
    {
        uint Read32(uint pa);
        void Write32(uint pa, uint v);
        byte Read8(uint pa);
        void Write8(uint pa, byte v);
    }

    public enum ArmMode : uint { Usr = 0x10, Fiq = 0x11, Irq = 0x12, Svc = 0x13, Mon = 0x16, Abt = 0x17, Hyp = 0x1A, Und = 0x1B, Sys = 0x1F }

    public sealed class ArmV7Core
    {
        public readonly uint[] R = new uint[16]; // current view; R[15] = PC of next fetch
        public uint Cpsr = (uint)ArmMode.Svc | 0xC0; // SVC, IRQ/FIQ masked (Linux boot contract)
        readonly Dictionary<ArmMode, (uint sp, uint lr, uint spsr)> _bank = new();
        readonly uint[] _usrHi = new uint[5], _fiqHi = new uint[5]; // r8-r12
        readonly IArmBus _bus;
        public long Steps { get; private set; }
        public event Action<string> Log;
        readonly HashSet<uint> _unimpl = new();

        public ArmV7Core(IArmBus bus) { _bus = bus; }

        public ArmMode Mode => (ArmMode)(Cpsr & 0x1F);
        public bool Thumb => (Cpsr & 0x20) != 0;
        bool N => (Cpsr & 0x80000000) != 0;
        bool Z => (Cpsr & 0x40000000) != 0;
        bool C => (Cpsr & 0x20000000) != 0;
        bool V => (Cpsr & 0x10000000) != 0;

        // Linux zImage entry: r0=0, r1=~0 (DT), r2=DTB phys, SVC, MMU off.
        public void BootLinux(uint entry, uint dtbPhys)
        {
            Array.Clear(R, 0, 16);
            R[1] = 0xFFFFFFFF; R[2] = dtbPhys; R[15] = entry;
            Cpsr = (uint)ArmMode.Svc | 0xC0;
        }

        public void SwitchMode(ArmMode to)
        {
            ArmMode from = Mode;
            if (from == to) return;
            _bank[Norm(from)] = (R[13], R[14], _bank.TryGetValue(Norm(from), out var b) ? b.spsr : 0);
            if (from == ArmMode.Fiq) { Array.Copy(R, 8, _fiqHi, 0, 5); Array.Copy(_usrHi, 0, R, 8, 5); }
            if (to == ArmMode.Fiq) { Array.Copy(R, 8, _usrHi, 0, 5); Array.Copy(_fiqHi, 0, R, 8, 5); }
            if (_bank.TryGetValue(Norm(to), out var nb)) { R[13] = nb.sp; R[14] = nb.lr; }
            Cpsr = (Cpsr & ~0x1Fu) | (uint)to;
        }
        static ArmMode Norm(ArmMode m) => m == ArmMode.Sys ? ArmMode.Usr : m;

        public bool CondPassed(uint cond) => cond switch
        {
            0x0 => Z, 0x1 => !Z, 0x2 => C, 0x3 => !C, 0x4 => N, 0x5 => !N, 0x6 => V, 0x7 => !V,
            0x8 => C && !Z, 0x9 => !C || Z, 0xA => N == V, 0xB => N != V,
            0xC => !Z && N == V, 0xD => Z || N != V, _ => true, // 0xE AL; 0xF unconditional space
        };

        public void Step(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Steps++;
                if (Thumb) { Unimpl(R[15], 0, "T32"); return; }
                uint pc = R[15];
                uint insn = _bus.Read32(pc); // MMU off phase: VA == PA
                R[15] = pc + 4;
                uint cond = insn >> 28;
                if (cond != 0xF && !CondPassed(cond)) continue;
                if ((insn & 0x0E000000) == 0x0A000000) // B / BL
                {
                    int off = (int)(insn << 8) >> 6;
                    if ((insn & 0x01000000) != 0) R[14] = pc + 4;
                    R[15] = (uint)(pc + 8 + off);
                    continue;
                }
                Unimpl(pc, insn, "A32");
                return;
            }
        }

        void Unimpl(uint pc, uint insn, string set)
        {
            if (_unimpl.Add(insn)) Log?.Invoke($"ARMV7 unimplemented {set} 0x{insn:X8} at 0x{pc:X8}");
            R[15] = pc; // stay; caller decides
        }
    }
}
