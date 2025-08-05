using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace ProcessorEmulator
{
    /// <summary>
    /// Windows CE API emulator for translating system calls to x64 host operations
    /// </summary>
    public class WindowsCEApiEmulator
    {
        private readonly Dictionary<uint, Func<CPUState, VirtualMemoryManager, Task<ExecutionResult>>> apiHandlers;
        private readonly Dictionary<string, uint> dllImports;
        private readonly Dictionary<uint, string> addressToApiName;

        public WindowsCEApiEmulator()
        {
            apiHandlers = new Dictionary<uint, Func<CPUState, VirtualMemoryManager, Task<ExecutionResult>>>();
            dllImports = new Dictionary<string, uint>();
            addressToApiName = new Dictionary<uint, string>();
            InitializeApiHandlers();
        }

        private void InitializeApiHandlers()
        {
            // Kernel32.dll APIs
            RegisterApi("GetTickCount", HandleGetTickCount);
            RegisterApi("Sleep", HandleSleep);
            RegisterApi("ExitProcess", HandleExitProcess);
            RegisterApi("GetCurrentThreadId", HandleGetCurrentThreadId);
            RegisterApi("GetCurrentProcessId", HandleGetCurrentProcessId);
            RegisterApi("CreateFileW", HandleCreateFileW);
            RegisterApi("ReadFile", HandleReadFile);
            RegisterApi("WriteFile", HandleWriteFile);
            RegisterApi("CloseHandle", HandleCloseHandle);
            RegisterApi("GetFileSize", HandleGetFileSize);
            RegisterApi("SetFilePointer", HandleSetFilePointer);
            RegisterApi("DeleteFileW", HandleDeleteFileW);
            RegisterApi("CreateDirectoryW", HandleCreateDirectoryW);
            RegisterApi("FindFirstFileW", HandleFindFirstFileW);
            RegisterApi("FindNextFileW", HandleFindNextFileW);
            RegisterApi("FindClose", HandleFindClose);

            // User32.dll APIs
            RegisterApi("MessageBoxW", HandleMessageBoxW);
            RegisterApi("GetDC", HandleGetDC);
            RegisterApi("ReleaseDC", HandleReleaseDC);
            RegisterApi("CreateWindowExW", HandleCreateWindowExW);
            RegisterApi("ShowWindow", HandleShowWindow);
            RegisterApi("UpdateWindow", HandleUpdateWindow);
            RegisterApi("GetMessageW", HandleGetMessageW);
            RegisterApi("DispatchMessageW", HandleDispatchMessageW);
            RegisterApi("PostQuitMessage", HandlePostQuitMessage);

            // GDI32.dll APIs
            RegisterApi("CreateCompatibleDC", HandleCreateCompatibleDC);
            RegisterApi("SelectObject", HandleSelectObject);
            RegisterApi("BitBlt", HandleBitBlt);
            RegisterApi("SetPixel", HandleSetPixel);
            RegisterApi("GetPixel", HandleGetPixel);
            RegisterApi("CreateBitmap", HandleCreateBitmap);
            RegisterApi("DeleteObject", HandleDeleteObject);

            // CRT APIs
            RegisterApi("malloc", HandleMalloc);
            RegisterApi("free", HandleFree);
            RegisterApi("memcpy", HandleMemcpy);
            RegisterApi("memset", HandleMemset);
            RegisterApi("strcpy", HandleStrcpy);
            RegisterApi("strlen", HandleStrlen);
            RegisterApi("printf", HandlePrintf);

            // Windows CE specific
            RegisterApi("CeGetSystemInfo", HandleCeGetSystemInfo);
            RegisterApi("CeGetVersionEx", HandleCeGetVersionEx);
            RegisterApi("CeCreateProcess", HandleCeCreateProcess);
        }

        private void RegisterApi(string name, Func<CPUState, VirtualMemoryManager, Task<ExecutionResult>> handler)
        {
            // Use hash of API name as virtual address
            var hash = (uint)name.GetHashCode();
            var virtualAddr = 0x80000000u + (hash & 0x7FFFFFF0u); // Ensure it's in kernel space

            apiHandlers[virtualAddr] = handler;
            dllImports[name] = virtualAddr;
            addressToApiName[virtualAddr] = name;
        }

        public uint GetApiAddress(string apiName)
        {
            return dllImports.TryGetValue(apiName, out var address) ? address : 0;
        }

        public uint GetFunctionAddress(string dllName, string functionName)
        {
            // For simplicity, ignore DLL name and just look up by function name
            return GetApiAddress(functionName);
        }

        public uint GetStubAddress()
        {
            // Return a generic stub address that will trigger API emulation
            return 0x80000000;
        }

        public async Task<ExecutionResult> HandleFunctionCallAsync(uint address, CPUState cpu, VirtualMemoryManager memory)
        {
            if (apiHandlers.TryGetValue(address, out var handler))
            {
                Console.WriteLine($"🔧 Calling Windows CE API: {addressToApiName[address]} at 0x{address:X8}");
                return await handler(cpu, memory);
            }

            // Unknown function call - treat as return
            Console.WriteLine($"⚠️ Unknown function call to 0x{address:X8}");
            return new ExecutionResult { ShouldExit = false };
        }

        public async Task<ExecutionResult> HandleSystemCallAsync(uint syscallNumber, CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine($"🔧 System call: {syscallNumber}");

            // Windows CE system calls are typically handled through APIs
            // For now, treat as NOP
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        // Kernel32.dll API Handlers
        private async Task<ExecutionResult> HandleGetTickCount(CPUState cpu, VirtualMemoryManager memory)
        {
            var ticks = (uint)Environment.TickCount;
            cpu.Registers[0] = ticks; // Return value in R0/V0
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleSleep(CPUState cpu, VirtualMemoryManager memory)
        {
            var milliseconds = cpu.Registers[0]; // First parameter
            await Task.Delay((int)Math.Min(milliseconds, 5000)); // Cap at 5 seconds
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleExitProcess(CPUState cpu, VirtualMemoryManager memory)
        {
            var exitCode = (int)cpu.Registers[0];
            Console.WriteLine($"🚪 Process exiting with code: {exitCode}");
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = true, ExitCode = exitCode };
        }

        private async Task<ExecutionResult> HandleGetCurrentThreadId(CPUState cpu, VirtualMemoryManager memory)
        {
            cpu.Registers[0] = 1234; // Fake thread ID
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleGetCurrentProcessId(CPUState cpu, VirtualMemoryManager memory)
        {
            cpu.Registers[0] = 5678; // Fake process ID
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleCreateFileW(CPUState cpu, VirtualMemoryManager memory)
        {
            var filenamePtr = cpu.Registers[0];
            var filename = ReadWideString(memory, filenamePtr);
            
            Console.WriteLine($"📁 CreateFileW: {filename}");
            
            // Return fake handle
            cpu.Registers[0] = 0x12345678;
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleReadFile(CPUState cpu, VirtualMemoryManager memory)
        {
            var handle = cpu.Registers[0];
            var buffer = cpu.Registers[1];
            var bytesToRead = cpu.Registers[2];
            var bytesReadPtr = cpu.Registers[3];
            
            Console.WriteLine($"📖 ReadFile: handle=0x{handle:X8}, bytes={bytesToRead}");
            
            // Fake successful read
            if (bytesReadPtr != 0)
            {
                memory.WriteUInt32(bytesReadPtr, Math.Min(bytesToRead, 1024));
            }
            cpu.Registers[0] = 1; // TRUE
            
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleWriteFile(CPUState cpu, VirtualMemoryManager memory)
        {
            var handle = cpu.Registers[0];
            var buffer = cpu.Registers[1];
            var bytesToWrite = cpu.Registers[2];
            var bytesWrittenPtr = cpu.Registers[3];
            
            Console.WriteLine($"📝 WriteFile: handle=0x{handle:X8}, bytes={bytesToWrite}");
            
            // Try to read and display the data being written
            if (buffer != 0 && bytesToWrite > 0 && bytesToWrite < 1024)
            {
                try
                {
                    var data = memory.ReadBytes(buffer, bytesToWrite);
                    var text = Encoding.UTF8.GetString(data).TrimEnd('\0');
                    Console.WriteLine($"📝 Writing: {text}");
                }
                catch
                {
                    // Ignore read errors
                }
            }
            
            // Fake successful write
            if (bytesWrittenPtr != 0)
            {
                memory.WriteUInt32(bytesWrittenPtr, bytesToWrite);
            }
            cpu.Registers[0] = 1; // TRUE
            
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleCloseHandle(CPUState cpu, VirtualMemoryManager memory)
        {
            var handle = cpu.Registers[0];
            Console.WriteLine($"🔒 CloseHandle: 0x{handle:X8}");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleGetFileSize(CPUState cpu, VirtualMemoryManager memory)
        {
            var handle = cpu.Registers[0];
            Console.WriteLine($"📏 GetFileSize: 0x{handle:X8}");
            cpu.Registers[0] = 1024; // Fake file size
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleSetFilePointer(CPUState cpu, VirtualMemoryManager memory)
        {
            var handle = cpu.Registers[0];
            var distanceToMove = cpu.Registers[1];
            var moveMethod = cpu.Registers[3];
            
            Console.WriteLine($"📍 SetFilePointer: handle=0x{handle:X8}, distance={distanceToMove}, method={moveMethod}");
            cpu.Registers[0] = distanceToMove; // Return new position
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleDeleteFileW(CPUState cpu, VirtualMemoryManager memory)
        {
            var filenamePtr = cpu.Registers[0];
            var filename = ReadWideString(memory, filenamePtr);
            
            Console.WriteLine($"🗑️ DeleteFileW: {filename}");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleCreateDirectoryW(CPUState cpu, VirtualMemoryManager memory)
        {
            var dirNamePtr = cpu.Registers[0];
            var dirName = ReadWideString(memory, dirNamePtr);
            
            Console.WriteLine($"📁 CreateDirectoryW: {dirName}");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleFindFirstFileW(CPUState cpu, VirtualMemoryManager memory)
        {
            var patternPtr = cpu.Registers[0];
            var pattern = ReadWideString(memory, patternPtr);
            
            Console.WriteLine($"🔍 FindFirstFileW: {pattern}");
            if (cpu.Registers != null && cpu.Registers.Length > 0) cpu.Registers[0] = 0xFFFFFFFF; // INVALID_HANDLE_VALUE (no files found)
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleFindNextFileW(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🔍 FindNextFileW");
            cpu.Registers[0] = 0; // FALSE (no more files)
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleFindClose(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🔍 FindClose");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        // User32.dll API Handlers
        private async Task<ExecutionResult> HandleMessageBoxW(CPUState cpu, VirtualMemoryManager memory)
        {
            var hwnd = cpu.Registers[0];
            var textPtr = cpu.Registers[1];
            var captionPtr = cpu.Registers[2];
            var type = cpu.Registers[3];
            
            var text = ReadWideString(memory, textPtr);
            var caption = ReadWideString(memory, captionPtr);
            
            Console.WriteLine($"💬 MessageBox: [{caption}] {text}");
            cpu.Registers[0] = 1; // IDOK
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleGetDC(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🖼️ GetDC");
            cpu.Registers[0] = 0x87654321; // Fake DC handle
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleReleaseDC(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🖼️ ReleaseDC");
            cpu.Registers[0] = 1; // Success
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        // Memory management APIs
        private async Task<ExecutionResult> HandleMalloc(CPUState cpu, VirtualMemoryManager memory)
        {
            var size = cpu.Registers[0];
            Console.WriteLine($"🧠 malloc: {size} bytes");
            
            // Allocate in high memory area
            var address = 0x70000000u + size; // Fake allocation
            cpu.Registers[0] = address;
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleFree(CPUState cpu, VirtualMemoryManager memory)
        {
            var ptr = cpu.Registers[0];
            Console.WriteLine($"🧠 free: 0x{ptr:X8}");
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleMemcpy(CPUState cpu, VirtualMemoryManager memory)
        {
            var dest = cpu.Registers[0];
            var src = cpu.Registers[1];
            var count = cpu.Registers[2];
            
            Console.WriteLine($"📋 memcpy: 0x{dest:X8} <- 0x{src:X8} ({count} bytes)");
            
            try
            {
                var data = memory.ReadBytes(src, count);
                memory.WriteBytes(dest, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ memcpy failed: {ex.Message}");
            }
            
            cpu.Registers[0] = dest; // Return destination
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleMemset(CPUState cpu, VirtualMemoryManager memory)
        {
            var dest = cpu.Registers[0];
            var value = (byte)(cpu.Registers[1] & 0xFF);
            var count = cpu.Registers[2];
            
            Console.WriteLine($"📋 memset: 0x{dest:X8} = 0x{value:X2} ({count} bytes)");
            
            try
            {
                var data = new byte[count];
                for (int i = 0; i < count; i++)
                    data[i] = value;
                memory.WriteBytes(dest, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ memset failed: {ex.Message}");
            }
            
            cpu.Registers[0] = dest; // Return destination
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        // String functions
        private async Task<ExecutionResult> HandleStrcpy(CPUState cpu, VirtualMemoryManager memory)
        {
            var dest = cpu.Registers[0];
            var src = cpu.Registers[1];
            
            var str = ReadCString(memory, src);
            Console.WriteLine($"📋 strcpy: \"{str}\"");
            
            try
            {
                WriteCString(memory, dest, str);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ strcpy failed: {ex.Message}");
            }
            
            cpu.Registers[0] = dest; // Return destination
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleStrlen(CPUState cpu, VirtualMemoryManager memory)
        {
            var str = cpu.Registers[0];
            var length = 0u;
            
            try
            {
                while (memory.ReadByte(str + length) != 0)
                    length++;
            }
            catch
            {
                // Handle error gracefully
            }
            
            Console.WriteLine($"📏 strlen: {length}");
            cpu.Registers[0] = length;
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandlePrintf(CPUState cpu, VirtualMemoryManager memory)
        {
            var formatPtr = cpu.Registers[0];
            var format = ReadCString(memory, formatPtr);
            
            Console.WriteLine($"🖨️ printf: \"{format}\"");
            cpu.Registers[0] = (uint)format.Length; // Return characters printed
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        // Windows CE specific APIs
        private async Task<ExecutionResult> HandleCeGetSystemInfo(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("ℹ️ CeGetSystemInfo");
            // Fill in fake system info structure
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleCeGetVersionEx(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("ℹ️ CeGetVersionEx");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleCeCreateProcess(CPUState cpu, VirtualMemoryManager memory)
        {
            var appNamePtr = cpu.Registers[0];
            var appName = ReadWideString(memory, appNamePtr);
            
            Console.WriteLine($"🚀 CeCreateProcess: {appName}");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        // Additional placeholder handlers for graphics and window APIs
        private async Task<ExecutionResult> HandleCreateWindowExW(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🪟 CreateWindowExW");
            cpu.Registers[0] = 0x11111111; // Fake window handle
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleShowWindow(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🪟 ShowWindow");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleUpdateWindow(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🪟 UpdateWindow");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleGetMessageW(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("📨 GetMessageW");
            cpu.Registers[0] = 0; // No messages (causes app to exit message loop)
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleDispatchMessageW(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("📨 DispatchMessageW");
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandlePostQuitMessage(CPUState cpu, VirtualMemoryManager memory)
        {
            var exitCode = (int)cpu.Registers[0];
            Console.WriteLine($"🚪 PostQuitMessage: {exitCode}");
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = true, ExitCode = exitCode };
        }

        // GDI placeholder handlers
        private async Task<ExecutionResult> HandleCreateCompatibleDC(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🎨 CreateCompatibleDC");
            cpu.Registers[0] = 0x22222222; // Fake DC handle
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleSelectObject(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🎨 SelectObject");
            cpu.Registers[0] = 0x33333333; // Fake previous object
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleBitBlt(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🎨 BitBlt");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleSetPixel(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🎨 SetPixel");
            cpu.Registers[0] = 0; // Success
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleGetPixel(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🎨 GetPixel");
            cpu.Registers[0] = 0x00FF00FF; // Fake pixel color
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleCreateBitmap(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🎨 CreateBitmap");
            cpu.Registers[0] = 0x44444444; // Fake bitmap handle
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        private async Task<ExecutionResult> HandleDeleteObject(CPUState cpu, VirtualMemoryManager memory)
        {
            Console.WriteLine("🎨 DeleteObject");
            cpu.Registers[0] = 1; // TRUE
            await Task.CompletedTask;
            return new ExecutionResult { ShouldExit = false };
        }

        // Helper methods for string operations
        private string ReadCString(VirtualMemoryManager memory, uint address)
        {
            var bytes = new List<byte>();
            uint offset = 0;
            
            try
            {
                while (true)
                {
                    var b = memory.ReadByte(address + offset);
                    if (b == 0) break;
                    bytes.Add(b);
                    offset++;
                    if (offset > 1024) break; // Safety limit
                }
            }
            catch
            {
                // Handle error gracefully
            }
            
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private string ReadWideString(VirtualMemoryManager memory, uint address)
        {
            var chars = new List<char>();
            uint offset = 0;
            
            try
            {
                while (true)
                {
                    var word = memory.ReadUInt16(address + offset);
                    if (word == 0) break;
                    chars.Add((char)word);
                    offset += 2;
                    if (offset > 2048) break; // Safety limit
                }
            }
            catch
            {
                // Handle error gracefully
            }
            
            return new string(chars.ToArray());
        }

        private void WriteCString(VirtualMemoryManager memory, uint address, string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            memory.WriteBytes(address, bytes);
            memory.WriteByte(address + (uint)bytes.Length, 0); // Null terminator
        }
    }
}
