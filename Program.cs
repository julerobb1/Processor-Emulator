using System;
using System.Collections.Generic;
using System.IO;
using ProcessorEmulator.Core;
using ProcessorEmulator.Core.Emulation;
using ProcessorEmulator.Core.Loaders;
using ProcessorEmulator.Core.Backends;

public class Program
{
    /// <summary>
    /// Creates a minimal, valid nk.bin file for testing the loader.
    /// </summary>
    static void CreateDummyNkBin(string filePath)
    {
        using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            writer.Write(new byte[] { 0x42, 0x30, 0x30, 0x30, 0x46, 0x46, 0x0A }); // "B000FF\n" 
            writer.Write((uint)0x80000000); // Image Start
            writer.Write((uint)0x10000);    // Image Length

            uint recordAddress = 0x00030000;
            writer.Write(recordAddress); // Record Address
            writer.Write((uint)4);       // Record Length
            writer.Write((uint)0);       // Record Checksum
            writer.Write(0x24020001);     // Instruction: ADDIU r2, r0, 1

            writer.Write((uint)0); // End-of-records marker
            writer.Write((uint)0);
            writer.Write(recordAddress); // Entry point
        }
    }

    /// <summary>
    /// Verifies that the MMU correctly translates KSEG0 and KSEG1 addresses.
    /// </summary>
    static bool RunMmuValidationTest(ICpuState state, IMemoryManager memory, IExecutionEngine executor)
    {
        Console.WriteLine("\n--- Running MMU Boundary Test ---");
        const uint physicalAddress = 0x00001000;
        const uint magicValue = 0xDEADC0DE;
        bool pass = true;

        memory.WriteMemory32(physicalAddress, magicValue);
        Console.WriteLine($"Wrote 0x{magicValue:X} to physical addr 0x{physicalAddress:X}");

        ulong kseg0Address = 0x80000000 | physicalAddress;
        var loadFromKseg0 = new IrStatement { Op = IrOpCode.Load, Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = "r1" }, SourceA = new IrOperand { IsImmediate = true, Value = kseg0Address }};
        executor.ExecuteStatement(loadFromKseg0, state, memory);
        ulong valFromKseg0 = state.GetRegister("r1", BitWidth.Bits32);

        Console.WriteLine($"Loading from KSEG0 0x{kseg0Address:X}... Got 0x{valFromKseg0:X}");
        if (valFromKseg0 != magicValue) {
            Console.WriteLine("  [FAIL] KSEG0 value mismatch!");
            pass = false;
        }

        ulong kseg1Address = 0xA0000000 | physicalAddress;
        var loadFromKseg1 = new IrStatement { Op = IrOpCode.Load, Destination = new IrOperand { Width = BitWidth.Bits32, RegisterName = "r2" }, SourceA = new IrOperand { IsImmediate = true, Value = kseg1Address }};
        executor.ExecuteStatement(loadFromKseg1, state, memory);
        ulong valFromKseg1 = state.GetRegister("r2", BitWidth.Bits32);

        Console.WriteLine($"Loading from KSEG1 0x{kseg1Address:X}... Got 0x{valFromKseg1:X}");
        if (valFromKseg1 != magicValue) {
            Console.WriteLine("  [FAIL] KSEG1 value mismatch!");
            pass = false;
        }

        Console.WriteLine(pass ? "[SUCCESS] MMU Test Passed." : "[FATAL] MMU Test Failed.");
        Console.WriteLine("-----------------------------------");
        return pass;
    }

    /// <summary>
    /// Verifies that the CP0 timer interrupt correctly triggers an exception.
    /// </summary>
    static bool RunInterruptValidationTest()
    {
        Console.WriteLine("\n--- Running Interrupt Validation Test ---");
        var state = new CpuState(new List<MemoryRegion> { new MemoryRegion("RAM", 0, 1024*1024, MemoryRegionType.RAM) }, false, 0x80000000);
        var runner = new IrRunner(state, state);
        bool pass = false;

        state.SetRegister("cp0_count", 0, BitWidth.Bits32);
        state.SetRegister("cp0_compare", 1000, BitWidth.Bits32);
        state.SetRegister("cp0_status", 1, BitWidth.Bits32); // Enable Interrupts
        
        // Write a NOP to memory so the CPU has something to execute
        state.WriteMemory32(state.PC, 0x00000000);

        Console.WriteLine("State Initialized: Count=0, Compare=1000, Status(IEc)=1. Looping...");

        for (int i = 0; i < 15; i++)
        {
            runner.Step();
            if (i == 9 && state.PC == 0x80000180) {
                pass = true;
                break;
            } else if (i == 9) {
                Console.WriteLine($"[FAIL] On step 10, PC is 0x{state.PC:X8}, expected exception vector.");
                break;
            }
        }
        
        Console.WriteLine(pass ? "[SUCCESS] Interrupt Test Passed." : "[FATAL] Interrupt Test Failed.");
        Console.WriteLine("---------------------------------------");
        return pass;
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("--- System Emulator Boot ---");
        
        var memoryMap = new List<MemoryRegion> { new MemoryRegion("RAM", 0x00000000, 64 * 1024 * 1024, MemoryRegionType.RAM) };
        var cpuState = new CpuState(memoryMap, isLittleEndian: false);
        var executor = new BaseIrExecutor();
        
        if (!RunMmuValidationTest(cpuState, cpuState, executor)) return;
        if (!RunInterruptValidationTest()) return;

        string kernelPath = "nk.bin";
        CreateDummyNkBin(kernelPath);
        Console.WriteLine($"\nCreated dummy kernel at '{kernelPath}'");

        try
        {
            ulong entryPoint = NkBinLoader.Load(kernelPath, cpuState);
            cpuState.PC = 0x80000000 | entryPoint; // Jump to KSEG0 virtual address
            Console.WriteLine($"Kernel loaded. PC set to virtual address 0x{cpuState.PC:X}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FATAL] Error loading kernel: {ex.Message}");
            return;
        }

        var runner = new IrRunner(cpuState, cpuState);
        Console.WriteLine("Execution engine initialized. Starting simulation loop...");
        Console.WriteLine("----------------------------------------------------\n");

        for (int i = 0; i < 5; i++)
        {
            try
            {
                runner.Step();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! EXECUTION HALTED: {ex.Message}");
                break;
            }
        }
        
        Console.WriteLine("\n--- Simulation Complete ---");
    }
}
