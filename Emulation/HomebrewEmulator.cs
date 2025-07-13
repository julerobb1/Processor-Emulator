using System;
using System.Windows;

namespace ProcessorEmulator.Emulation
{
    /// <summary>
    /// A homebrew CPU/OS emulator stub. Attempts to run firmware natively.
    /// Falls back to QEMU when not implemented or unsupported.
    /// </summary>
    public class HomebrewEmulator : IEmulator
    {
        private readonly string architecture;
        private byte[] memory;
        private uint[] regs = new uint[32];
        private uint pc;
        private Tools.DeviceTreeManager.Node deviceTree;

        public HomebrewEmulator(string architecture)
        {
            this.architecture = architecture;
        }

        public void LoadBinary(byte[] binary)
        {
            // Load firmware into memory (256MB)
            memory = new byte[256 * 1024 * 1024];
            Array.Copy(binary, 0, memory, 0, binary.Length);
            // Detect Flattened Device Tree (FDT) blob in memory (magic 0xd00dfeed)
            for (int off = 0; off + 4 < binary.Length; off += 4)
            {
                uint magic = BitConverter.ToUInt32(memory, off);
                if (magic == 0xd00dfeed)
                {
                    byte[] dtb = new byte[binary.Length - off];
                    Array.Copy(memory, off, dtb, 0, dtb.Length);
                    deviceTree = Tools.DeviceTreeManager.Load(dtb);
                    break;
                }
            }
            // Initialize registers and PC
            for (int i = 0; i < regs.Length; i++) regs[i] = 0;
            pc = 0;
        }

        public void Run()
        {
            // Dispatch by architecture
            if (architecture.StartsWith("MIPS", StringComparison.OrdinalIgnoreCase))
            {
                // MIPS32 fetch-decode-execute
                const int maxSteps = 1000000;
                for (int step = 0; step < maxSteps; step++)
                    Step();
                return;
            }
            else if (architecture.StartsWith("ARM", StringComparison.OrdinalIgnoreCase))
            {
                // Delegate to ARM emulator stub
                var armEmu = new ArmEmulator();
                armEmu.LoadBinary(memory);
                armEmu.Run();
                return;
            }
            // Add other ISA dispatch as needed
            // Unknown or unsupported architecture: warn and fallback to QEMU
            MessageBox.Show(
                $"Homebrew emulator not implemented for architecture: {architecture}.\nFalling back to QEMU emulation.",
                "Homebrew Emulator", MessageBoxButton.OK, MessageBoxImage.Warning);
            // Throw to signal MainWindow to catch and launch QEMU
            throw new NotImplementedException($"Homebrew emulator not implemented for architecture: {architecture}");
        }
        
        public void Step()
        {
            // Fetch 32-bit instruction
            uint instr = BitConverter.ToUInt32(memory, (int)pc);
            uint opcode = instr >> 26;
            switch (opcode)
            {
                case 0: // R-type
                    {
                        uint rs = (instr >> 21) & 0x1F;
                        uint rt = (instr >> 16) & 0x1F;
                        uint rd = (instr >> 11) & 0x1F;
                        uint funct = instr & 0x3F;
                        if (funct == 0x20) // ADD
                            regs[rd] = regs[rs] + regs[rt];
                        else if (funct == 0x22) // SUB
                            regs[rd] = regs[rs] - regs[rt];
                        else
                            throw new NotImplementedException($"R-type funct {funct:X} not implemented");
                        pc += 4;
                        break;
                    }
                case 0x08: // ADDI
                    {
                        uint rs = (instr >> 21) & 0x1F;
                        uint rt = (instr >> 16) & 0x1F;
                        short imm = (short)(instr & 0xFFFF);
                        regs[rt] = regs[rs] + (uint)imm;
                        pc += 4;
                        break;
                    }
                case 0x23: // LW
                    {
                        uint baseR = (instr >> 21) & 0x1F;
                        uint rt = (instr >> 16) & 0x1F;
                        short offset = (short)(instr & 0xFFFF);
                        uint addr = regs[baseR] + (uint)offset;
                        regs[rt] = BitConverter.ToUInt32(memory, (int)addr);
                        pc += 4;
                        break;
                    }
                case 0x2B: // SW
                    {
                        uint baseR = (instr >> 21) & 0x1F;
                        uint rt = (instr >> 16) & 0x1F;
                        short offset = (short)(instr & 0xFFFF);
                        uint addr = regs[baseR] + (uint)offset;
                        var data = BitConverter.GetBytes(regs[rt]);
                        Array.Copy(data, 0, memory, addr, 4);
                        pc += 4;
                        break;
                    }
                case 0x04: // BEQ
                    {
                        uint rs = (instr >> 21) & 0x1F;
                        uint rt = (instr >> 16) & 0x1F;
                        short offset = (short)(instr & 0xFFFF);
                        if (regs[rs] == regs[rt])
                            pc += (uint)(offset << 2) + 4;
                        else
                            pc += 4;
                        break;
                    }
                case 0x05: // BNE
                    {
                        uint rs = (instr >> 21) & 0x1F;
                        uint rt = (instr >> 16) & 0x1F;
                        short offset = (short)(instr & 0xFFFF);
                        if (regs[rs] != regs[rt])
                            pc += (uint)(offset << 2) + 4;
                        else
                            pc += 4;
                        break;
                    }
                case 0x02: // J
                    {
                        uint target = instr & 0x03FFFFFF;
                        pc = (pc & 0xF0000000) | (target << 2);
                        break;
                    }
                default:
                    throw new NotImplementedException($"Opcode {opcode:X} not implemented");
            }
        }

        public void Decompile()
        {
            // Simple raw disassembly output
            for (uint addr = 0; addr < memory.Length; addr += 4)
            {
                uint instr = BitConverter.ToUInt32(memory, (int)addr);
                Console.WriteLine($"{addr:X8}: {instr:X8}");
            }
        }

        public void Recompile(string targetArch)
        {
            throw new NotImplementedException("Recompile not implemented");
        }
    }
}
