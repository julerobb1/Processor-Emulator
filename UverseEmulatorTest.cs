using System;
using System.Threading.Tasks;
using ProcessorEmulator.Emulation;

namespace ProcessorEmulator
{
    /// <summary>
    /// Quick test class for U-verse MIPS emulator
    /// </summary>
    public class UverseEmulatorTest
    {
        public static async Task RunTest()
        {
            Console.WriteLine("=== U-verse MIPS Emulator Test ===");
            Console.WriteLine();

            try
            {
                // Create the MIPS U-verse emulator
                var mipsEmulator = new MipsUverseEmulator();
                Console.WriteLine($"Created emulator: {mipsEmulator.ChipsetName}");

                // Initialize the emulator
                Console.WriteLine("Initializing emulator...");
                if (!mipsEmulator.Initialize(""))
                {
                    Console.WriteLine("❌ Failed to initialize MIPS emulator");
                    return;
                }
                Console.WriteLine("✅ Emulator initialized successfully");

                // Start emulation
                Console.WriteLine("Starting emulation...");
                await mipsEmulator.StartEmulation();
                Console.WriteLine("✅ Emulation started");

                // Get status
                Console.WriteLine("\n=== Emulator Status ===");
                var status = mipsEmulator.GetStatus();
                
                foreach (var kvp in status)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }

                Console.WriteLine("\n=== Test Completed Successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during test: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Entry point for standalone testing (disabled: multiple entry points not allowed)
        // public static async Task Main(string[] args)
        // {
        //     if (args.Length > 0 && args[0] == "--test-uverse")
        //     {
        //         await RunTest();
        //         Console.WriteLine("Press any key to exit...");
        //         Console.ReadKey();
        //         Environment.Exit(0);
        //     }
        // }
    }
}
