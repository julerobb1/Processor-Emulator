using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading;

namespace ProcessorEmulator
{
    /// <summary>
    /// Information about a running Windows CE process
    /// </summary>
    public class ProcessInfo
    {
        public string ProcessId { get; set; }
        public string ExePath { get; set; }
        public PEArchitecture Architecture { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public bool IsRunning { get; set; }
        public int ExitCode { get; set; }
        public TimeSpan RunTime => (StopTime ?? DateTime.Now) - StartTime;
    }

    /// <summary>
    /// Result of Windows CE binary execution
    /// </summary>
    public class WindowsCEExecutionResult
    {
        public bool Success { get; set; }
        public PEArchitecture Architecture { get; set; }
        public uint EntryPoint { get; set; }
        public int ExitCode { get; set; }
        public string Error { get; set; }
        public List<string> Log { get; set; } = new List<string>();
        public string ProcessId { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    /// <summary>
    /// Windows CE PE Executable Cross-Platform Executor
    /// Runs Windows CE ARM/MIPS binaries on x64 hosts through binary translation
    /// Supports concurrent execution of multiple processes
    /// </summary>
    public class WindowsCEExecutor
    {
        private readonly Dictionary<string, ProcessContext> runningProcesses;
        private readonly PEImageLoader peLoader;
        private readonly InstructionTranslator translator;
        private readonly WindowsCEApiEmulator apiEmulator;
        private readonly object processLock = new object();

        public WindowsCEExecutor()
        {
            runningProcesses = new Dictionary<string, ProcessContext>();
            peLoader = new PEImageLoader();
            translator = new InstructionTranslator();
            apiEmulator = new WindowsCEApiEmulator();
        }

        /// <summary>
        /// Get list of currently running processes
        /// </summary>
        public List<ProcessInfo> GetRunningProcesses()
        {
            lock (processLock)
            {
                return runningProcesses.Values.Select(p => new ProcessInfo
                {
                    ProcessId = p.ProcessId,
                    ExePath = p.ExePath,
                    Architecture = p.PEImage?.Architecture ?? PEArchitecture.Unknown,
                    StartTime = p.StartTime,
                    IsRunning = p.IsRunning,
                    ExitCode = p.ExitCode
                }).ToList();
            }
        }

        /// <summary>
        /// Execute multiple Windows CE binaries concurrently
        /// </summary>
        public async Task<List<WindowsCEExecutionResult>> ExecuteMultipleAsync(string[] exePaths, string[][] args = null)
        {
            var tasks = new List<Task<WindowsCEExecutionResult>>();
            
            for (int i = 0; i < exePaths.Length; i++)
            {
                var path = exePaths[i];
                var processArgs = args?[i];
                
                // Launch each process in parallel
                tasks.Add(Task.Run(async () => await ExecuteAsync(path, processArgs)));
            }

            return (await Task.WhenAll(tasks)).ToList();
        }

        /// <summary>
        /// Stop a running process by process ID
        /// </summary>
        public bool StopProcess(string processId)
        {
            lock (processLock)
            {
                if (runningProcesses.TryGetValue(processId, out var context))
                {
                    context.IsRunning = false;
                    context.ExitCode = -1;
                    context.StopTime = DateTime.Now;
                    Console.WriteLine($"🛑 Stopped process: {processId}");
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Stop all running processes
        /// </summary>
        public void StopAllProcesses()
        {
            lock (processLock)
            {
                foreach (var context in runningProcesses.Values.Where(p => p.IsRunning))
                {
                    context.IsRunning = false;
                    context.ExitCode = -1;
                    context.StopTime = DateTime.Now;
                }
                Console.WriteLine($"🛑 Stopped all {runningProcesses.Count} processes");
            }
        }

        /// <summary>
        /// Execute Windows CE binary on x64 host
        /// </summary>
        public async Task<WindowsCEExecutionResult> ExecuteAsync(string exePath, string[] args = null)
        {
            var startTime = DateTime.Now;
            var result = new WindowsCEExecutionResult();
            var log = result.Log;

            try
            {
                log.Add($"🔧 Loading Windows CE executable: {Path.GetFileName(exePath)}");
                
                // Step 1: Load and analyze PE file
                var peImage = await peLoader.LoadPEImageAsync(exePath);
                if (peImage == null)
                {
                    result.Error = "Failed to load PE image";
                    log.Add("❌ Failed to load PE image");
                    return result;
                }

                result.Architecture = peImage.Architecture;
                result.EntryPoint = peImage.EntryPoint;

                // Step 2: Detect architecture and validate
                log.Add($"📊 Architecture: {peImage.Architecture}");
                log.Add($"🎯 Entry Point: 0x{peImage.EntryPoint:X8}");
                log.Add($"💾 Image Base: 0x{peImage.ImageBase:X8}");
                log.Add($"📦 Subsystem: {peImage.Subsystem}");

                if (peImage.Subsystem != PESubsystem.WindowsCE)
                {
                    log.Add("⚠️ Not a Windows CE executable, attempting generic PE execution...");
                }

                // Step 3: Create process context with thread-safe ID generation
                var processId = $"wince_{Path.GetFileNameWithoutExtension(exePath)}_{DateTime.Now.Ticks}_{Thread.CurrentThread.ManagedThreadId}";
                result.ProcessId = processId;
                
                var context = new ProcessContext
                {
                    ProcessId = processId,
                    ExePath = exePath,
                    Arguments = args ?? new string[0],
                    PEImage = peImage,
                    VirtualMemory = new VirtualMemoryManager(),
                    IsRunning = true,
                    StartTime = startTime
                };

                // Register process in thread-safe manner
                lock (processLock)
                {
                    runningProcesses[processId] = context;
                }

                log.Add($"🆔 Process ID: {processId}");

                // Step 4: Set up virtual memory space
                log.Add("🗺️ Setting up virtual memory space...");
                await SetupVirtualMemoryAsync(context);

                // Step 5: Load import table and resolve APIs
                log.Add("🔗 Resolving imports...");
                await ResolveImportsAsync(context);

                // Step 6: Start execution
                log.Add("🚀 Starting execution...");
                var exitCode = await ExecuteProcessAsync(context);

                // Step 7: Clean up and return results
                lock (processLock)
                {
                    if (runningProcesses.ContainsKey(processId))
                    {
                        context.IsRunning = false;
                        context.ExitCode = exitCode;
                        context.StopTime = DateTime.Now;
                    }
                }

                result.ExitCode = exitCode;
                result.Success = exitCode == 0;
                result.ExecutionTime = DateTime.Now - startTime;
                log.Add($"✅ Process completed with exit code: {exitCode} in {result.ExecutionTime.TotalMilliseconds:F0}ms");
                
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.Success = false;
                result.ExecutionTime = DateTime.Now - startTime;
                log.Add($"❌ Execution failed: {ex.Message}");
                
                // Clean up failed process
                if (!string.IsNullOrEmpty(result.ProcessId))
                {
                    lock (processLock)
                    {
                        if (runningProcesses.TryGetValue(result.ProcessId, out var context))
                        {
                            context.IsRunning = false;
                            context.ExitCode = -1;
                            context.StopTime = DateTime.Now;
                        }
                    }
                }
                
                return result;
            }
        }

        private async Task SetupVirtualMemoryAsync(ProcessContext context)
        {
            var vm = context.VirtualMemory;
            var pe = context.PEImage;

            // Map image to virtual memory
            vm.MapRegion(pe.ImageBase, pe.SizeOfImage, MemoryProtection.ReadWrite);

            // Map stack (1MB default)
            var stackBase = 0x7FF00000u;
            vm.MapRegion(stackBase, 0x100000, MemoryProtection.ReadWrite);
            context.StackPointer = stackBase + 0x100000 - 4; // Top of stack

            // Map heap (16MB)
            var heapBase = 0x10000000u;
            vm.MapRegion(heapBase, 0x1000000, MemoryProtection.ReadWrite);

            // Copy sections to virtual memory
            foreach (var section in pe.Sections)
            {
                if (section.VirtualSize > 0)
                {
                    var virtualAddr = pe.ImageBase + section.VirtualAddress;
                    Console.WriteLine($"  📂 Section {section.Name}: 0x{virtualAddr:X8} ({section.VirtualSize} bytes)");
                    
                    if (section.RawData != null)
                    {
                        vm.WriteBytes(virtualAddr, section.RawData);
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task ResolveImportsAsync(ProcessContext context)
        {
            var pe = context.PEImage;
            var vm = context.VirtualMemory;

            foreach (var import in pe.Imports)
            {
                Console.WriteLine($"  📚 DLL: {import.DllName}");
                
                foreach (var function in import.Functions)
                {
                    // Get emulated function address
                    var funcAddress = apiEmulator.GetFunctionAddress(import.DllName, function.Name);
                    
                    if (funcAddress != 0)
                    {
                        // Write function address to IAT
                        vm.WriteUInt32(function.IATAddress, funcAddress);
                        Console.WriteLine($"    ✅ {function.Name} -> 0x{funcAddress:X8}");
                    }
                    else
                    {
                        Console.WriteLine($"    ⚠️ {function.Name} -> Unresolved");
                        // Write stub address for unsupported functions
                        vm.WriteUInt32(function.IATAddress, apiEmulator.GetStubAddress());
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task<int> ExecuteProcessAsync(ProcessContext context)
        {
            try
            {
                // Initialize CPU state
                var cpuState = new CPUState
                {
                    PC = context.PEImage.EntryPoint,
                    SP = context.StackPointer,
                    Architecture = context.PEImage.Architecture
                };

                // Set up initial registers based on architecture
                if (context.PEImage.Architecture == PEArchitecture.ARM)
                {
                    // ARM calling convention
                    cpuState.Registers[0] = 0; // argc equivalent
                    cpuState.Registers[1] = context.StackPointer - 0x1000; // argv equivalent
                    cpuState.Registers[13] = context.StackPointer; // Stack pointer
                    cpuState.Registers[14] = 0; // Link register (return address)
                }
                else if (context.PEImage.Architecture == PEArchitecture.MIPS)
                {
                    // MIPS calling convention
                    cpuState.Registers[4] = 0; // $a0 = argc
                    cpuState.Registers[5] = context.StackPointer - 0x1000; // $a1 = argv
                    cpuState.Registers[29] = context.StackPointer; // $sp = stack pointer
                    cpuState.Registers[31] = 0; // $ra = return address
                }

                Console.WriteLine($"🎯 Starting execution at 0x{cpuState.PC:X8}");

                // Main execution loop
                var executedInstructions = 0;
                var maxInstructions = 1000000; // Safety limit

                while (context.IsRunning && executedInstructions < maxInstructions)
                {
                    try
                    {
                        // Fetch instruction
                        var instruction = context.VirtualMemory.ReadUInt32(cpuState.PC);
                        
                        // Translate and execute
                        var result = await translator.TranslateAndExecuteAsync(
                            instruction, cpuState, context.VirtualMemory, apiEmulator);

                        if (result.ShouldExit)
                        {
                            Console.WriteLine($"📊 Process exiting with code: {result.ExitCode}");
                            return result.ExitCode;
                        }

                        if (result.NewPC != 0)
                        {
                            cpuState.PC = result.NewPC;
                        }
                        else
                        {
                            // Default: increment PC
                            cpuState.PC += (cpuState.Architecture == PEArchitecture.MIPS) ? 4u : 
                                           (cpuState.Architecture == PEArchitecture.ARM) ? 4u : 1u;
                        }

                        executedInstructions++;

                        // Progress reporting
                        if (executedInstructions % 50000 == 0)
                        {
                            Console.WriteLine($"📈 Executed {executedInstructions} instructions, PC: 0x{cpuState.PC:X8}");
                        }
                    }
                    catch (MemoryAccessException ex)
                    {
                        Console.WriteLine($"💥 Memory access violation at 0x{ex.Address:X8}: {ex.Message}");
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"💥 Execution error at PC 0x{cpuState.PC:X8}: {ex.Message}");
                        return -1;
                    }
                }

                if (executedInstructions >= maxInstructions)
                {
                    Console.WriteLine("⏰ Execution limit reached");
                    return -2;
                }

                Console.WriteLine($"✅ Program completed normally after {executedInstructions} instructions");
                return 0;
            }
            finally
            {
                context.IsRunning = false;
                runningProcesses.Remove(context.ProcessId);
            }
        }

        public void ListRunningProcesses()
        {
            Console.WriteLine("\n📋 Running Windows CE Processes:");
            if (runningProcesses.Count == 0)
            {
                Console.WriteLine("  (none)");
                return;
            }

            foreach (var kvp in runningProcesses)
            {
                var context = kvp.Value;
                var runtime = DateTime.Now - context.StartTime;
                Console.WriteLine($"  🔧 {Path.GetFileName(context.ExePath)} (PID: {kvp.Key})");
                Console.WriteLine($"     Runtime: {runtime.TotalSeconds:F1}s, Arch: {context.PEImage.Architecture}");
            }
        }

        public void TerminateProcess(string processId)
        {
            if (runningProcesses.TryGetValue(processId, out var context))
            {
                context.IsRunning = false;
                Console.WriteLine($"🛑 Terminated process: {processId}");
            }
        }

        public void TerminateAll()
        {
            foreach (var context in runningProcesses.Values)
            {
                context.IsRunning = false;
            }
            runningProcesses.Clear();
            Console.WriteLine("🛑 Terminated all processes");
        }
    }

    public class ProcessContext
    {
        public string ProcessId { get; set; }
        public string ExePath { get; set; }
        public string[] Arguments { get; set; }
        public PEImageInfo PEImage { get; set; }
        public VirtualMemoryManager VirtualMemory { get; set; }
        public uint StackPointer { get; set; }
        public bool IsRunning { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public int ExitCode { get; set; }
    }

    public class CPUState
    {
        public uint PC { get; set; } // Program Counter
        public uint SP { get; set; } // Stack Pointer
        public uint[] Registers { get; set; } = new uint[32]; // General purpose registers
        public uint CPSR { get; set; } // Current Program Status Register (ARM)
        public PEArchitecture Architecture { get; set; }
    }

    public class ExecutionResult
    {
        public bool ShouldExit { get; set; }
        public int ExitCode { get; set; }
        public uint NewPC { get; set; }
    }

    public class MemoryAccessException : Exception
    {
        public uint Address { get; }
        public MemoryAccessException(uint address, string message) : base(message)
        {
            Address = address;
        }
    }
}
