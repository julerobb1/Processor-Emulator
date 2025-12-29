using System;

class Program
{
    static void Main()
    {
        var cpu = new MipsCpuEmulator();

        byte[] program = new byte[1024];
        uint beq = (4u << 26) | (1u << 21) | (2u << 16) | (2u & 0xFFFFu);
        Array.Copy(BitConverter.GetBytes(beq), 0, program, 0, 4);
        uint addi1 = (8u << 26) | (0u << 21) | (3u << 16) | (10u & 0xFFFFu);
        Array.Copy(BitConverter.GetBytes(addi1), 0, program, 4, 4);
        uint addi2 = (8u << 26) | (0u << 21) | (4u << 16) | (20u & 0xFFFFu);
        Array.Copy(BitConverter.GetBytes(addi2), 0, program, 8, 4);

        cpu.LoadProgram(program, 0);
        cpu.SetRegister(1, 5);
        cpu.SetRegister(2, 5);

        Console.WriteLine("Executing Branch...");
        cpu.Step(1);

        Console.WriteLine($"Register 3 (Delay Slot): {cpu.GetRegister(3)} (Expected 10)");
        Console.WriteLine($"PC: {cpu.ProgramCounter} (Expected 8)");

        if (cpu.GetRegister(3) == 10 && cpu.ProgramCounter == 8)
        {
            Console.WriteLine("TEST PASSED: Branch delay slot executed correctly.");
        }
        else
        {
            Console.WriteLine("TEST FAILED.");
            Environment.Exit(2);
        }
    }
}
