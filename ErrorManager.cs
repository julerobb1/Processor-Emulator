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
        // Error Code Constants
        public const int ERROR_GENERAL = 1000;
        public const int ERROR_FILE_NOT_FOUND = 1001;
        public const int ERROR_INVALID_FIRMWARE = 1002;
        public const int ERROR_EMULATION_FAILED = 1003;
        public const int ERROR_MEMORY_ACCESS = 1004;
        public const int ERROR_CPU_EXCEPTION = 1005;
        public const int ERROR_HYPERVISOR_CRASH = 5001;
        public const int ERROR_LE_GRILLE = 5010;
        public const int ERROR_TRIED_AND_FAILED = 5020;
        public const int ERROR_TOWEL = 5030;

        // Error message dictionary with pop culture references
        private static readonly Dictionary<int, string> ErrorMessages = new Dictionary<int, string>
        {
            // General Errors with Simpsons flair
            { 1000, "D'oh! Something went wrong." },
            { 1001, "File not found: Like Waldo, but less fun to find." },
            { 1002, "Invalid firmware: This firmware is faker than Marge's natural hair color." },
            { 1003, "Emulation failed: Worst. Emulation. Ever." },
            { 1004, "Memory access violation: You've angered the memory gods." },
            { 1005, "CPU exception: Processor having an existential crisis." },
            
            // Hypervisor Errors
            { 5001, "💥 SYSTEM HALTED 💥\nCause: Someone poked reality too hard.\nSuggested Fix: Reverse your last three actions and say 'D'oh!'" },
            { 5002, "Virtual machine error: D'oh! VM exploded." },
            { 5003, "Hardware emulation failed: Snake jazz hardware." },
            { 5004, "Memory allocation error: Cleanup memory aisles." },
            { 5005, "CPU virtualization failed: Stupid sexy Flanders CPU." },
            
            // Special French Instructions Error (Homer meme reference)
            { 5010, "D'oh... English side ruined, must use French instructions." },
            { 5011, "Le Grille? What the hell is that?" },
            { 5012, "Instructions unclear: Put a six pack of beer in the fridge. It will be cold by the time you are through." },
            
            // Failure Philosophy Errors (Homer wisdom)
            { 5020, "You tried your best and you failed miserably. The lesson is, never try." },
            { 5021, "Kids, you tried your best and you failed miserably. The lesson is, never try." },
            { 5022, "Trying is the first step towards failure." },
            { 5023, "If something's hard to do, then it's not worth doing." },
            
            // Towel Error (Hitchhiker's Guide + Simpsons mashup)
            { 5030, "You'll have to speak up, I'm wearing a towel." },
            { 5031, "Error 42: Don't forget to bring a towel." },
            { 5032, "Panic! And don't know where your towel is." }
        };

        // Status messages for long operations
        private static readonly string[] FunnyStatusMessages = new string[]
        {
            "Warming up the hamsters...",
            "Convincing electrons to cooperate...",
            "Teaching CPU new dance moves...",
            "Bribing the firmware gods...",
            "Untangling spaghetti code...",
            "Feeding the magic smoke...",
            "Calibrating the flux capacitor...",
            "Reversing the polarity...",
            "Waking up sleepy processes...",
            "Defragmenting the temporal matrix..."
        };

        /// <summary>
        /// Show a standard error message with pop culture flair
        /// </summary>
        public static void ShowError(int errorCode, string additionalInfo = "")
        {
            string message = GetErrorMessage(errorCode);
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message += "\n\nAdditional Info: " + additionalInfo;
            }
            
            MessageBox.Show(message, $"Error {errorCode}", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        /// <summary>
        /// Show error with custom message and additional info (3-parameter overload)
        /// </summary>
        public static void ShowError(string customMessage, int errorCode, string additionalInfo)
        {
            string fullMessage = customMessage;
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                fullMessage += "\n\nAdditional Info: " + additionalInfo;
            }
            
            MessageBox.Show(fullMessage, $"Error {errorCode}", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        /// <summary>
        /// Show error with error code, additional info, and exception
        /// </summary>
        public static void ShowError(int errorCode, string additionalInfo, Exception ex)
        {
            string message = GetErrorMessage(errorCode);
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message += "\n\nAdditional Info: " + additionalInfo;
            }
            if (ex != null)
            {
                message += "\n\nException: " + ex.Message;
            }
            
            MessageBox.Show(message, $"Error {errorCode}", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Show hypervisor crash with special formatting
        /// </summary>
        public static void ShowHypervisorCrash(string cause = "Someone poked reality too hard")
        {
            string message = $"💥 HYPERVISOR CRASH 💥\n\n" +
                           $"Cause: {cause}\n" +
                           $"Code: You'll have to speak up, I'm wearing a towel.\n\n" +
                           $"Status: System achieved maximum wonkiness.\n" +
                           $"Recommended Action: Turn it off and on again. If that fails, blame Carl.";
            
            MessageBox.Show(message, "System Halted", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        /// <summary>
        /// Show the classic "Le Grille" French instructions error
        /// </summary>
        public static void ShowLeGrilleError()
        {
            string message = "D'oh... English side ruined, must use French instructions.\n\n" +
                           "Le Grille? What the hell is that?\n\n" +
                           "Instructions unclear: Try reading the manual upside down while standing on one foot.";
            
            MessageBox.Show(message, "Instruction Translation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Show Homer's "tried and failed" philosophy
        /// </summary>
        public static void ShowTriedAndFailed(string operation = "that thing you just tried")
        {
            string message = $"You tried your best with '{operation}' and you failed miserably.\n\n" +
                           "The lesson is, never try.\n\n" +
                           "Homer's Wisdom: If something's hard to do, then it's not worth doing.";
            
            MessageBox.Show(message, "Philosophical Failure", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Show when instructions are unclear
        /// </summary>
        public static void ShowInstructionsUnclear(string operation = "that thing you're trying to do")
        {
            string message = $"Instructions unclear for: {operation}\n\n" +
                           "Put a six pack of beer in the fridge.\n" +
                           "It will be cold by the time you are through.\n\n" +
                           "Alternative: Step 1: Draw some circles. Step 2: Draw the rest of the owl.";
            
            MessageBox.Show(message, "Clarity Issues", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        /// <summary>
        /// Show towel-related errors (Hitchhiker's Guide + Simpsons)
        /// </summary>
        public static void ShowTowelError()
        {
            string message = "You'll have to speak up, I'm wearing a towel.\n\n" +
                           "Error 42: Don't forget to bring a towel.\n" +
                           "Status: Panic! And don't know where your towel is.\n\n" +
                           "Remember: A towel is the most massively useful thing any interstellar hitchhiker can carry.";
            
            MessageBox.Show(message, "Towel Protocol Violation", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        /// <summary>
        /// Get a random funny status message for long operations
        /// </summary>
        public static string GetRandomStatus()
        {
            Random rand = new Random();
            return FunnyStatusMessages[rand.Next(FunnyStatusMessages.Length)];
        }

        /// <summary>
        /// Get error message by code
        /// </summary>
        public static string GetErrorMessage(int errorCode)
        {
            if (ErrorMessages.TryGetValue(errorCode, out string message))
            {
                return message;
            }
            return $"Unknown error occurred. Error code: {errorCode}. D'oh!";
        }

        /// <summary>
        /// Show funny status for operations that might take a while
        /// </summary>
        public static void ShowFunnyStatus(string operation)
        {
            string funnyMessage = GetRandomStatus();
            MessageBox.Show($"{operation}\n\n{funnyMessage}\n\nThis might take a moment...", 
                          "Working...", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Show RetDec specific failure with Homer philosophy
        /// </summary>
        public static void ShowRetDecFailure(string details = "")
        {
            string message = "RetDec decompilation failed miserably.\n\n" +
                           "You tried your best and you failed miserably.\n" +
                           "The lesson is, never try... automatic decompilation.\n\n" +
                           "Alternative: Manual reverse engineering. It's like homework, but for grown-ups.";
            
            if (!string.IsNullOrEmpty(details))
            {
                message += $"\n\nTechnical Details: {details}";
            }
            
            MessageBox.Show(message, "Decompilation Philosophical Crisis", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Show cross-compilation errors with Simpsons flair
        /// </summary>
        public static void ShowCrossCompilationError(string target = "unknown architecture")
        {
            string message = $"Cross-compilation to {target} failed.\n\n" +
                           "Stupid sexy Flanders architecture!\n" +
                           "Nothing at all... nothing at all... nothing at all!\n\n" +
                           "Try: Different toolchain, or blame the compiler.";
            
            MessageBox.Show(message, "Compilation Identity Crisis", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Show filesystem mounting errors with pop culture references
        /// </summary>
        public static void ShowFilesystemError(string fsType = "SquashFS")
        {
            string message = $"{fsType} mounting failed.\n\n" +
                           "Filesystem is more compressed than Homer's understanding of quantum physics.\n" +
                           "Status: Worst. Mount. Ever.\n\n" +
                           "Try: Different extraction tool, or ask Moe for help.";
            
            MessageBox.Show(message, "Filesystem Rebellion", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        // Backward compatibility methods for legacy code
        public static class Codes
        {
            public const int HYPERVISOR_CRASH = ERROR_HYPERVISOR_CRASH;
            public const int MEMORY_ALLOCATION_ERROR = 5004;
            public const int HYPERVISOR_SUCCESS = 6001;
            public const int BOOT_SUCCESS = 6002;
            public const int EMULATION_SUCCESS = 6003;
            
            // All missing error codes
            public const int INVALID_PARAMETER = 1010;
            public const int INITIALIZATION_FAILED = 1012;
            public const int INITIALIZING = 2001;
            public const int PROCESSING = 2002;
            public const int LOADING = 2003;
            public const int ANALYZING = 2004;
            public const int WUBBA_SUCCESS = 6010;
            public const int SNAKE_JAZZ_SUCCESS = 6011;
            public const int BART_SUCCESS = 6012;
            public const int OPERATION_SUCCESS = 6013;
            public const int WELCOME_MESSAGE = 3001;
            public const int FILE_NOT_FOUND = ERROR_FILE_NOT_FOUND;
            public const int ACCESS_DENIED = 1020;
            public const int INVALID_FIRMWARE_FORMAT = 1021;
            public const int EMULATION_FAILED = ERROR_EMULATION_FAILED;
            public const int DATA_CORRUPTION = 1030;
            public const int GENERAL_FAILURE = 1031;
            public const int FILESYSTEM_CORRUPTION = 1040;
            public const int MOUNT_FAILED = 1041;
            public const int TRIED_AND_FAILED = ERROR_TRIED_AND_FAILED;
            public const int BEER_FRIDGE_INSTRUCTIONS = 5012;
        }
        
        public static string GetStatusMessage(int errorCode)
        {
            return ErrorMessages.ContainsKey(errorCode) ? ErrorMessages[errorCode] : "Unknown status";
        }
        
        public static string GetSuccessMessage(int successCode)
        {
            var successMessages = new Dictionary<int, string>
            {
                { 6001, "✅ Hypervisor running smoothly" },
                { 6002, "✅ Boot sequence completed" },
                { 6003, "✅ Emulation successful" }
            };
            return successMessages.ContainsKey(successCode) ? successMessages[successCode] : "Success!";
        }
        
        public static void ShowSuccess(int successCode, string details = "")
        {
            string message = GetSuccessMessage(successCode);
            if (!string.IsNullOrEmpty(details))
                message += $"\n\nDetails: {details}";
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        public static void LogError(int errorCode, string details, Exception ex = null)
        {
            string logMessage = $"[ERROR {errorCode}] {GetStatusMessage(errorCode)}";
            if (!string.IsNullOrEmpty(details))
                logMessage += $" - {details}";
            if (ex != null)
                logMessage += $" - Exception: {ex.Message}";
            
            Console.WriteLine(logMessage);
            // Could also write to file here if needed
        }
        
        public static void ShowHypervisorCrash(string operation, Exception ex)
        {
            string message = $"💥 HYPERVISOR CRASH 💥\n\n" +
                           $"Operation: {operation}\n" +
                           $"Error: {ex.Message}\n\n" +
                           "D'oh! Someone poked reality too hard.\n" +
                           "Suggested fix: Reverse your last three actions and say 'D'oh!'";
            
            MessageBox.Show(message, "Hypervisor Meltdown", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
