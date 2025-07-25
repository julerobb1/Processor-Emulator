using System;
using System.Collections.Generic;
using System.Windows;

namespace ProcessorEmulator
{
    /// <summary>
    /// Centralized error management with pop culture references and proper error codes
    /// </summary>
    public static class ErrorManager
    {
        // Error code ranges:
        // 1000-1999: System/General errors
        // 2000-2999: Filesystem errors  
        // 3000-3999: Emulation errors
        // 4000-4999: Network/Communication errors
        // 5000-5999: Hardware/Hypervisor errors

        private static readonly Dictionary<int, string> ErrorMessages = new Dictionary<int, string>
        {
            // System Integrity Errors (1000-1099)
            { 1001, "Eat my shorts, system integrity not found." },
            { 1002, "Wubba lubba dub dub! You broke reality again." },
            { 1003, "Ahh geez Rick, something went wrong." },
            { 1004, "D'oh! That didn't work as expected." },
            { 1005, "Stupid sexy Flanders broke the system." },
            
            // Kernel/Core Errors (1100-1199)
            { 1101, "Kernel Panic: Wubba lubba dub dub! You broke reality again." },
            { 1102, "Core system failure: Don't have a cow, man." },
            { 1103, "Critical error: Snake jazz isn't fixing this." },
            { 1104, "System crash: Cleanup on all the aisles." },
            { 1105, "Fatal error: That's a paddlin'." },
            
            // Invalid Operations (1200-1299)
            { 1201, "Invalid item selected: Snake Jazz" },
            { 1202, "Operation not supported: Ahh geez Rick." },
            { 1203, "Invalid parameter: Wubba lubba dub dub!" },
            { 1204, "Unauthorized access: Eat my shorts!" },
            { 1205, "Permission denied: Stupid sexy Flanders." },
            
            // Filesystem Errors (2000-2099)
            { 2001, "D'oh! Stupid sexy Flanders corrupting the filesystem." },
            { 2002, "File not found: Snake jazz can't locate that." },
            { 2003, "Disk full: Cleanup on all the aisles needed." },
            { 2004, "Corrupted data: Wubba lubba dub dub file system!" },
            { 2005, "Access denied: Don't have a cow, man." },
            
            // Mount/Unmount Errors (2100-2199)
            { 2101, "Mount failed: Ahh geez Rick, can't mount that." },
            { 2102, "Unmount error: Stupid sexy Flanders won't let go." },
            { 2103, "Filesystem busy: Snake jazz is playing on it." },
            { 2104, "Invalid filesystem: D'oh! That's not right." },
            { 2105, "Permission error: Eat my shorts, access denied." },
            
            // Emulation Errors (3000-3099)
            { 3001, "Emulation failed: Wubba lubba dub dub CPU!" },
            { 3002, "ARM decode error: Ahh geez Rick, bad instruction." },
            { 3003, "Memory fault: Don't have a cow about segfaults." },
            { 3004, "Register corruption: Snake jazz in the CPU." },
            { 3005, "Execution halt: Stupid sexy Flanders stopped it." },
            
            // Firmware Errors (3100-3199)
            { 3101, "Firmware load failed: D'oh! Bad binary." },
            { 3102, "Boot sequence error: Eat my shorts, bootloader!" },
            { 3103, "Kernel panic in firmware: Wubba lubba dub dub!" },
            { 3104, "Firmware corruption: Cleanup on aisle firmware." },
            { 3105, "Invalid firmware format: Ahh geez Rick." },
            
            // Network/QEMU Errors (4000-4099)
            { 4001, "QEMU launch failed: Snake jazz won't start." },
            { 4002, "Network error: Stupid sexy Flanders blocked it." },
            { 4003, "Connection timeout: Don't have a cow, it's slow." },
            { 4004, "Protocol error: Wubba lubba dub dub packets!" },
            { 4005, "Service unavailable: Ahh geez Rick, it's down." },
            
            // Hypervisor Errors (5000-5099)
            { 5001, "Hypervisor crash: Wubba lubba dub dub reality!" },
            { 5002, "Virtual machine error: D'oh! VM exploded." },
            { 5003, "Hardware emulation failed: Snake jazz hardware." },
            { 5004, "Memory allocation error: Cleanup memory aisles." },
            { 5005, "CPU virtualization failed: Stupid sexy Flanders CPU." }
        };

        private static readonly Dictionary<int, string> SuccessMessages = new Dictionary<int, string>
        {
            { 9001, "Welcome steady customer." },
            { 9002, "Excellent! *Mr. Burns voice*" },
            { 9003, "Wubba lubba dub dub! Success!" },
            { 9004, "Don't have a cow, man - it worked!" },
            { 9005, "Snake jazz success melody." },
            { 9006, "Stupid sexy Flanders... succeeded perfectly." },
            { 9007, "That's a successful paddlin'." },
            { 9008, "Ahh geez Rick, it actually worked!" },
            { 9009, "D'oh! I mean... success!" },
            { 9010, "Everything's coming up Milhouse!" }
        };

        private static readonly Dictionary<int, string> StatusMessages = new Dictionary<int, string>
        {
            { 8001, "Initializing... don't have a cow, man." },
            { 8002, "Processing... wubba lubba dub dub!" },
            { 8003, "Loading... snake jazz playing in background." },
            { 8004, "Analyzing... stupid sexy Flanders analyzing." },
            { 8005, "Extracting... cleanup on data aisles." },
            { 8006, "Mounting... ahh geez Rick, this takes time." },
            { 8007, "Emulating... D'oh! Virtual circuits activating." },
            { 8008, "Translating... binary snake jazz conversion." },
            { 8009, "Decompiling... wubba lubba dub dub assembly!" },
            { 8010, "Scanning... don't have a cow during analysis." }
        };

        /// <summary>
        /// Show an error message with proper error code
        /// </summary>
        public static void ShowError(int errorCode, string context = "", Exception ex = null)
        {
            string message = GetErrorMessage(errorCode);
            string fullMessage = $"Error {errorCode}: {message}";
            
            if (!string.IsNullOrEmpty(context))
                fullMessage += $"\n\nContext: {context}";
                
            if (ex != null)
                fullMessage += $"\n\nTechnical Details: {ex.Message}";
            
            MessageBox.Show(fullMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Show a success message
        /// </summary>
        public static void ShowSuccess(int successCode = 9001)
        {
            string message = GetSuccessMessage(successCode);
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Get error message by code
        /// </summary>
        public static string GetErrorMessage(int errorCode)
        {
            return ErrorMessages.TryGetValue(errorCode, out string message) 
                ? message 
                : "Unknown error: Ahh geez Rick, something's wrong.";
        }

        /// <summary>
        /// Get success message by code
        /// </summary>
        public static string GetSuccessMessage(int successCode)
        {
            return SuccessMessages.TryGetValue(successCode, out string message) 
                ? message 
                : "Success! Don't have a cow, man.";
        }

        /// <summary>
        /// Get status message for long operations
        /// </summary>
        public static string GetStatusMessage(int statusCode)
        {
            return StatusMessages.TryGetValue(statusCode, out string message) 
                ? message 
                : "Processing... snake jazz playing.";
        }

        /// <summary>
        /// Log error with code for debugging
        /// </summary>
        public static void LogError(int errorCode, string context = "", Exception ex = null)
        {
            string message = GetErrorMessage(errorCode);
            System.Diagnostics.Debug.WriteLine($"[ERROR {errorCode}] {message}");
            
            if (!string.IsNullOrEmpty(context))
                System.Diagnostics.Debug.WriteLine($"[CONTEXT] {context}");
                
            if (ex != null)
                System.Diagnostics.Debug.WriteLine($"[EXCEPTION] {ex}");
        }

        /// <summary>
        /// Get random status message for long operations
        /// </summary>
        public static string GetRandomStatusMessage()
        {
            var codes = new[] { 8001, 8002, 8003, 8004, 8005, 8006, 8007, 8008, 8009, 8010 };
            var random = new Random();
            return GetStatusMessage(codes[random.Next(codes.Length)]);
        }

        /// <summary>
        /// Get random success message
        /// </summary>
        public static string GetRandomSuccessMessage()
        {
            var codes = new[] { 9001, 9002, 9003, 9004, 9005, 9006, 9007, 9008, 9009, 9010 };
            var random = new Random();
            return GetSuccessMessage(codes[random.Next(codes.Length)]);
        }

        // Common error codes as constants for easy reference
        public static class Codes
        {
            // System Errors
            public const int SYSTEM_INTEGRITY_LOST = 1001;
            public const int REALITY_BROKEN = 1002;
            public const int GENERAL_FAILURE = 1003;
            public const int UNEXPECTED_ERROR = 1004;
            public const int SYSTEM_CORRUPTION = 1005;
            
            // Kernel Errors
            public const int KERNEL_PANIC = 1101;
            public const int CORE_FAILURE = 1102;
            public const int CRITICAL_ERROR = 1103;
            public const int SYSTEM_CRASH = 1104;
            public const int FATAL_ERROR = 1105;
            
            // Invalid Operations
            public const int INVALID_SELECTION = 1201;
            public const int UNSUPPORTED_OPERATION = 1202;
            public const int INVALID_PARAMETER = 1203;
            public const int UNAUTHORIZED_ACCESS = 1204;
            public const int PERMISSION_DENIED = 1205;
            
            // Filesystem Errors
            public const int FILESYSTEM_CORRUPTION = 2001;
            public const int FILE_NOT_FOUND = 2002;
            public const int DISK_FULL = 2003;
            public const int DATA_CORRUPTION = 2004;
            public const int ACCESS_DENIED = 2005;
            
            // Mount Errors
            public const int MOUNT_FAILED = 2101;
            public const int UNMOUNT_ERROR = 2102;
            public const int FILESYSTEM_BUSY = 2103;
            public const int INVALID_FILESYSTEM = 2104;
            public const int MOUNT_PERMISSION_ERROR = 2105;
            
            // Emulation Errors
            public const int EMULATION_FAILED = 3001;
            public const int ARM_DECODE_ERROR = 3002;
            public const int MEMORY_FAULT = 3003;
            public const int REGISTER_CORRUPTION = 3004;
            public const int EXECUTION_HALT = 3005;
            
            // Firmware Errors
            public const int FIRMWARE_LOAD_FAILED = 3101;
            public const int BOOT_SEQUENCE_ERROR = 3102;
            public const int FIRMWARE_KERNEL_PANIC = 3103;
            public const int FIRMWARE_CORRUPTION = 3104;
            public const int INVALID_FIRMWARE_FORMAT = 3105;
            
            // Network/QEMU Errors
            public const int QEMU_LAUNCH_FAILED = 4001;
            public const int NETWORK_ERROR = 4002;
            public const int CONNECTION_TIMEOUT = 4003;
            public const int PROTOCOL_ERROR = 4004;
            public const int SERVICE_UNAVAILABLE = 4005;
            
            // Hypervisor Errors
            public const int HYPERVISOR_CRASH = 5001;
            public const int VIRTUAL_MACHINE_ERROR = 5002;
            public const int HARDWARE_EMULATION_FAILED = 5003;
            public const int MEMORY_ALLOCATION_ERROR = 5004;
            public const int CPU_VIRTUALIZATION_FAILED = 5005;
            
            // Success Codes
            public const int WELCOME_MESSAGE = 9001;
            public const int OPERATION_SUCCESS = 9002;
            public const int WUBBA_SUCCESS = 9003;
            public const int BART_SUCCESS = 9004;
            public const int SNAKE_JAZZ_SUCCESS = 9005;
            
            // Status Codes
            public const int INITIALIZING = 8001;
            public const int PROCESSING = 8002;
            public const int LOADING = 8003;
            public const int ANALYZING = 8004;
            public const int EXTRACTING = 8005;
        }
    }
}
