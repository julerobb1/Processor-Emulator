using System;
using System.Collections.Generic;
using System.IO;
using ProcessorEmulator.Core;
using ProcessorEmulator.Core.Emulation;
using ProcessorEmulator.Core.Loaders;

public class Program
{
    /// <summary>
    /// Creates a minimal, valid nk.bin file for testing the loader.
    /// </summary>
    static void CreateDummyNkBin(string filePath)
    {
        using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            // Header
            writer.Write(new byte[] { 0x42, 0x30, 0x30, 0x30, 0x46, 0x46, 0x0A }); // "B000FF\n"
            uint imageStart = 0x80000000;
            uint imageLength = 0x10000; // Dummy length
            writer.Write(imageStart);
            writer.Write(imageLength);

            // First (and only) Record
            uint recordAddress = 0x00030000;
            uint recordLength = 4; // One MIPS instruction
            uint recordChecksum = 0; // Ignored for now
            writer.Write(recordAddress);
            writer.Write(recordLength);
            writer.Write(recordChecksum);

            // The instruction: ADDI r1, r0, 100 (0x20010064)
            writer.Write(0x20010064);

            // Sync / End Record
            writer.Write((uint)0); // Zero address
            writer.Write((uint)0); // Zero length
            writer.Write(recordAddress); // Entry point is the start of our code
        }
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("--- System Emulator Boot ---");
        
        // 1. Create a dummy kernel file for the test
        string kernelPath = "nk.bin";
        CreateDummyNkBin(kernelPath);
        Console.WriteLine($"Created dummy kernel at '{kernelPath}'");

        // 2. Define the machine's memory map
        var memoryMap = new List<MemoryRegion>
        {
            // 64MB of RAM starting at physical address 0
            new MemoryRegion("RAM", 0x00000000, 64 * 1024 * 1024, MemoryRegionType.RAM)
        };
        
        // 3. Initialize the CPU state and memory manager
        var cpuState = new CpuState(memoryMap, isLittleEndian: false);
        Console.WriteLine("Initialized CPU state and memory map.");

        // 4. Load the kernel into memory
        ulong entryPoint = 0;
        try
        {
            // The loader uses physical addresses.
            entryPoint = NkBinLoader.Load(kernelPath, cpuState);
            cpuState.PC = entryPoint;
            Console.WriteLine($"Kernel loaded. Entry point set to physical address 0x{entryPoint:X}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading kernel: {ex.Message}");
            return;
        }

        // 5. Initialize the execution engine
        var runner = new IrRunner(cpuState, cpuState);
        Console.WriteLine("Execution engine initialized. Starting simulation loop...");
        Console.WriteLine("---------------------------------------------------- ");

        // 6. Run the execution loop for a few steps
        for (int i = 0; i < 5; i++)
        {
            try
            {
                Console.WriteLine($"Step {i+1}: PC=0x{cpuState.PC:X8}");
                runner.Step();

                // Print out the changed register to see the result
                var r1_val = cpuState.GetRegister("r1", BitWidth.Bits32);
                Console.WriteLine($"  -> r1 = {r1_val} (0x{r1_val:X8})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! EXECUTION HALTED: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                break;
            }
        }
        
        Console.WriteLine("\n--- Simulation Complete ---");
    }
}