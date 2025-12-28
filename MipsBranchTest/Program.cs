using System;
using ProcessorEmulator.Emulation;

class Program
{
    // MIPS register numbers: $t0=8, $t1=9, $t2=10
    static void Main()
    {
        var emu = new MipsCpuEmulator();

        // Build simple program:
        // 0x00: beq $t0,$t1, offset=3   (if equal, target = pcAfterFetch + (3<<2) => 0x10)
        // 0x04: addi $t2,$t2,5          (delay slot)
        // 0x08: nop
        // 0x0C: nop
        // 0x10: nop (branch target)

        uint beq = (0x04u << 26) | (8u << 21) | (9u << 16) | (uint)((ushort)3);
        uint addi = (0x08u << 26) | (10u << 21) | (10u << 16) | (uint)((ushort)5);
        uint nop = 0u;

        byte[] program = new byte[20];
        Array.Copy(BitConverter.GetBytes(beq), 0, program, 0, 4);
        Array.Copy(BitConverter.GetBytes(addi), 0, program, 4, 4);
        Array.Copy(BitConverter.GetBytes(nop), 0, program, 8, 4);
        Array.Copy(BitConverter.GetBytes(nop), 0, program, 12, 4);
        Array.Copy(BitConverter.GetBytes(nop), 0, program, 16, 4);

        emu.LoadProgram(program, 0);

        // Set $t0 == $t1 so branch is taken
        emu.SetRegister(8, 1);
        emu.SetRegister(9, 1);
        emu.SetRegister(10, 0);

        // Execute three instructions: branch (fetches delay slot), delay slot, then target
        // We'll step 3 times and then inspect
        emu.Step(3);

        uint t2 = emu.GetRegister(10);
        uint pc = emu.ProgramCounter;

        Console.WriteLine($"$t2 = {t2}, PC = 0x{pc:X}");

        bool pass = (t2 == 5) && (pc == 0x10);
        Console.WriteLine(pass ? "TEST PASS" : "TEST FAIL");

        if (!pass) Environment.Exit(2);
    }
}
