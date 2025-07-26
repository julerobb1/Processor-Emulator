using System;
using System.Collections.Generic;
using System.Windows;
using ProcessorEmulator.CarlContainmentProtocol;

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
        
        // Rick and Morty Error Codes
        public const int ERROR_WUBBA_LUBBA_DUB_DUB = 6001;
        public const int ERROR_GEEZ_RICK = 6002;
        public const int ERROR_SNAKE_JAZZ = 6003;
        
        // Futurama Error Codes
        public const int ERROR_GOOD_NEWS_CRASH = 7001;
        
        // Additional Simpsons Codes
        public const int ERROR_EAT_MY_SHORTS = 8001;
        public const int ERROR_STEADY_CUSTOMER = 8002;
        public const int ERROR_DONT_HAVE_COW = 8003;

        // Compatibility constants for MainWindow
        public static class Codes
        {
            public const int INVALID_PARAMETER = ERROR_GENERAL;
            public const int INITIALIZING = ERROR_DONT_HAVE_COW;
            public const int PROCESSING = ERROR_SNAKE_JAZZ;
            public const int WUBBA_SUCCESS = ERROR_WUBBA_LUBBA_DUB_DUB;
            public const int WELCOME_MESSAGE = ERROR_STEADY_CUSTOMER;
            public const int FILE_NOT_FOUND = ERROR_FILE_NOT_FOUND;
            public const int OPERATION_SUCCESS = ERROR_STEADY_CUSTOMER;
            public const int BOOT_SEQUENCE_ERROR = ERROR_EMULATION_FAILED;
            public const int LOADING = ERROR_DONT_HAVE_COW;
            public const int HYPERVISOR_CRASH = ERROR_HYPERVISOR_CRASH;
            
            // Additional missing constants
            public const int ACCESS_DENIED = ERROR_MEMORY_ACCESS;
            public const int INVALID_FIRMWARE_FORMAT = ERROR_INVALID_FIRMWARE;
            public const int EMULATION_FAILED = ERROR_EMULATION_FAILED;
            public const int ANALYZING = ERROR_SNAKE_JAZZ;
            public const int DATA_CORRUPTION = ERROR_MEMORY_ACCESS;
            public const int GENERAL_FAILURE = ERROR_GENERAL;
            public const int SNAKE_JAZZ_SUCCESS = ERROR_SNAKE_JAZZ;
            public const int FILESYSTEM_CORRUPTION = ERROR_MEMORY_ACCESS;
            public const int MOUNT_FAILED = ERROR_FILE_NOT_FOUND;
            public const int BART_SUCCESS = ERROR_EAT_MY_SHORTS;
            public const int TRIED_AND_FAILED = ERROR_TRIED_AND_FAILED;
            public const int BEER_FRIDGE_INSTRUCTIONS = ERROR_LE_GRILLE;
            public const int MEMORY_ALLOCATION_ERROR = ERROR_MEMORY_ACCESS;
        }

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
            { 5003, "Hardware emulation failed: Snake jazz." },
            { 5004, "Memory allocation error: Cleanup on all the memory aisles." },
            { 5005, "CPU virtualization failed: Stupid Flanders." },
            
            // Special French Instructions Error (Homer meme reference)
            { 5010, "D'oh... Hex side ruined, must use Binary instructions." },
            { 5011, "Le null? What the hell is that?" },
            { 5012, "Instructions unclear: Put a six pack of beer in the fridge. It will be cold by the time you are through trying to do whatever it was you were trying to do." },
            
            // Failure Philosophy Errors (Homer wisdom)
            { 5020, "You tried your best and you failed miserably. The lesson is, never try." },
            { 5021, "Kid, you tried your best and the program still failed you. D'oh!" },
            { 5022, "failing and doing it successfully is the first step towards failure." },
            { 5023, "If something's hard to do, then it's not worth doing the conventional way." },
            
            // Towel Error (Hitchhiker's Guide + Simpsons mashup)
            { 5030, "You'll have to speak up, I'm wearing a towel." },
            { 5031, "Error 42: Where have all the answers gone?!." },
            { 5032, "Panic! Why? Because I said so." },
            
            // Rick and Morty Errors
            { 6001, "Wubba lubba dub dub! You broke reality again." },
            { 6002, "Ahh geez Rick." },
            { 6003, "Snake jazz hardware malfunction." },
            
            // Futurama Errors
            { 7001, "Good news everyone! I taught the program to crash!" },
            
            // Additional Simpsons Errors
            { 8001, "Eat my shorts, system integrity not found." },
            { 8002, "Welcome steady customer." },
            { 8003, "Initializing... don't have a cow, man." }, 
            { 8004, "D'oh! It actually worked!" },
            { 8005, "Good news everyone! Operation successful!" },
            { 8006, "SKIINNNER!" },
            { 8008, "This feature is still in development. Don't have a cow, man." },
            { 8009, "Aurora Borealis, in this program, at this time of year, at this time of day, in this part of the country, localized entirely within your device? That must some kind of magic!" }
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
            "Defragmenting the temporal matrix...",
            "Initializing... don't have a cow, man.",
            "Cleanup on all the aisles.",
            "Snake jazz processing...",
            "Ahh geez Rick, this is taking forever...",
            "Wubba lubba dub dub! Still working..."
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
        /// Show error with 3 parameters for MainWindow compatibility
        /// </summary>
        public static void ShowError(int errorCode, string operation, string details)
        {
            string message = GetErrorMessage(errorCode);
            message += $"\n\nOperation: {operation}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $"\nDetails: {details}";
            }
            
            MessageBox.Show(message, $"Error {errorCode}", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Show error with Exception for MainWindow compatibility
        /// </summary>
        public static void ShowError(int errorCode, string operation, Exception ex)
        {
            string message = GetErrorMessage(errorCode);
            message += $"\n\nOperation: {operation}";
            message += $"\nException: {ex.Message}";
            
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
        public static void ShowInstructionsUnclear(string operation = "")
        {
            string message = "Instructions unclear.\n\n" +
                           "Put a six pack of beer in the fridge.\n" +
                           "It will be cold by the time you are through.\n\n" +
                           "Alternative: Step 1: Draw some circles. Step 2: Draw the rest of the owl.";
            
            if (!string.IsNullOrEmpty(operation))
            {
                message = $"Instructions unclear for '{operation}'.\n\n" + message;
            }
            
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
        /// Get error message by code (public for MainWindow compatibility)
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

        /// <summary>
        /// Show Rick and Morty style kernel panic
        /// </summary>
        public static void ShowKernelPanic()
        {
            string message = "Wubba lubba dub dub! You broke reality again.\n\n" +
                           "Kernel Status: Completely morty-fied\n" +
                           "Recovery Options: Try turning it off and on again... burp...";
            
            MessageBox.Show(message, "Kernel Panic", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Show Rick and Morty style general error
        /// </summary>
        public static void ShowGeezRickError(string operation = "whatever you just tried")
        {
            string message = $"Ahh geez Rick, {operation} didn't work.\n\n" +
                           "Status: Everything is chrome in the future!\n" +
                           "Suggestion: Maybe try something less ambitious?";
            
            MessageBox.Show(message, "Rick and Morty Mishap", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Show Futurama style crash announcement
        /// </summary>
        public static void ShowFuturamaStyleCrash()
        {
            string message = "Good news everyone! I taught the program to crash!\n\n" +
                           "Professor Farnsworth would be proud.\n" +
                           "Status: Science has gone too far!";
            
            MessageBox.Show(message, "Scientific Achievement", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Show Bart Simpson style system error
        /// </summary>
        public static void ShowEatMyShortsError()
        {
            string message = "Eat my shorts, system integrity not found.\n\n" +
                           "Status: System is having a cow, man.\n" +
                           "Suggested Action: Don't have a cow yourself.";
            
            MessageBox.Show(message, "Bart Simpson System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Show welcome message for first-time users
        /// </summary>
        public static void ShowWelcomeSteadyCustomer()
        {
            string message = "Welcome steady customer!\n\n" +
                           "Congratulations on your first successful file extraction or useful interaction!\n" +
                           "Moe's Tavern Quality Service: Now with 50% less incompetence!";
            
            MessageBox.Show(message, "Achievement Unlocked", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Show filesystem corruption with Flanders reference
        /// </summary>
        public static void ShowFilesystemCorruption()
        {
            string message = "D'oh! Stupid sexy Flanders corrupting the filesystem.\n\n" +
                           "Status: Filesystem is feeling stupid... stupid... stupid...\n" +
                           "Recovery: Nothing at all... nothing at all... nothing at all!";
            
            MessageBox.Show(message, "Filesystem Corruption", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Get a tooltip message for unimplemented features
        /// </summary>
        public static string GetUnimplementedFeatureTooltip()
        {
            string[] tooltips = new string[]
            {
                "This feature is still in development. Don't have a cow, man!",
                "Ahh geez Rick, this button doesn't do anything yet.",
                "Good news everyone! This feature will crash spectacularly when implemented!",
                "Eat my shorts! Feature coming soon.",
                "Wubba lubba dub dub! Still working on this one.",
                "D'oh! This feature is as incomplete as Homer's diet plan.",
                "Snake jazz development in progress...",
                "Stupid sexy Flanders... this feature isn't ready yet.",
                "You'll have to speak up, this feature is wearing a towel."
            };
            
            Random rand = new Random();
            return tooltips[rand.Next(tooltips.Length)];
        }

        /// <summary>
        /// Get status message for MainWindow compatibility
        /// </summary>
        public static string GetStatusMessage(int code)
        {
            string[] statusMessages = new string[]
            {
                "Initializing... don't have a cow, man.",
                "Snake jazz processing...",
                "Ahh geez Rick, this is taking forever...",
                "Wubba lubba dub dub! Still working...",
                "D'oh! Processing stuff..."
            };
            
            Random rand = new Random();
            return statusMessages[rand.Next(statusMessages.Length)];
        }

        /// <summary>
        /// Get success message for MainWindow compatibility
        /// </summary>
        public static string GetSuccessMessage(int code)
        {
            string[] successMessages = new string[]
            {
                "Welcome steady customer!",
                "Wubba lubba dub dub! Success!",
                "D'oh! It actually worked!",
                "Good news everyone! Operation successful!",
                "Snake jazz complete!"
            };
            
            Random rand = new Random();
            return successMessages[rand.Next(successMessages.Length)];
        }

        /// <summary>
        /// Show success message for MainWindow compatibility
        /// </summary>
        public static void ShowSuccess(int code, string message = "")
        {
            string successMsg = GetSuccessMessage(code);
            if (!string.IsNullOrEmpty(message))
            {
                successMsg += "\n\n" + message;
            }
            
            MessageBox.Show(successMsg, "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Log error for MainWindow compatibility
        /// </summary>
        public static void LogError(int code, string details = "")
        {
            // For now, just show the error since we don't have a logging system
            ShowError(code, details);
        }

        /// <summary>
        /// Log error with 3 parameters for MainWindow compatibility
        /// </summary>
        public static void LogError(int code, string operation, string details)
        {
            // For now, just show the error since we don't have a logging system
            ShowError(code, operation, details);
        }

        /// <summary>
        /// Log error with Exception for MainWindow compatibility
        /// </summary>
        public static void LogError(int code, string operation, Exception ex)
        {
            // For now, just show the error since we don't have a logging system
            ShowError(code, operation, ex);
        }
        
        #region Carl-Specific Emergency Protocols
        
        /// <summary>
        /// Handle Carl-induced system failures with appropriate containment measures
        /// </summary>
        public static void ShowCarlEmergency(string incident)
        {
            var threat = CarlMonitor.CurrentThreatLevel;
            var message = threat switch
            {
                CarlMonitor.CarlThreatLevel.Critical => $"🚨 CARL CRITICAL INCIDENT 🚨\n\n{incident}\n\n" +
                    "Emergency Protocol Activated:\n" +
                    "• All buttons have been hidden\n" +
                    "• Coffee supply secured\n" +
                    "• Llama barriers deployed\n" +
                    "• Strongly worded memo sent\n\n" +
                    "System attempting self-recovery...",
                
                CarlMonitor.CarlThreatLevel.Apocalyptic => $"💥 CARL APOCALYPTIC EVENT 💥\n\n{incident}\n\n" +
                    "🦙 INTERDIMENSIONAL LLAMAS DETECTED! 🦙\n\n" +
                    "Emergency Actions Taken:\n" +
                    "• Dimensional vacuum deployed\n" +
                    "• Emergency hay distributed\n" +
                    "• All personnel evacuated to safe dimensions\n" +
                    "• Carl's access privileges revoked (again)\n\n" +
                    "May the force be with us all.",
                
                _ => $"⚠️ CARL INCIDENT DETECTED ⚠️\n\n{incident}\n\n" +
                    "Don't panic! This is probably just Carl being Carl.\n" +
                    "Containment protocols are in effect."
            };
            
            MessageBox.Show(message, "🔴 CARL CONTAINMENT PROTOCOL", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        /// <summary>
        /// Special llama-specific error handling
        /// </summary>
        public static void ShowLlamaAlert(LlamaException ex)
        {
            var message = $"🦙 LLAMA DIMENSIONAL BREACH 🦙\n\n" +
                $"Exception: {ex.Message}\n\n" +
                "Emergency Llama Protocols:\n" +
                "• Deploying emergency hay\n" +
                "• Activating dimensional barriers\n" +
                "• Sending Carl to timeout corner\n" +
                "• Consulting llama whisperer\n\n" +
                "Please stand by while we contain the llama situation.\n" +
                "Do NOT feed the interdimensional llamas!";
            
            MessageBox.Show(message, "🦙 LLAMA CONTAINMENT PROTOCOL", 
                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            
            // Generate incident report
            var report = CarlMonitor.GetCarlIncidentReport();
            var reportText = string.Join("\n", report);
            
            // Show detailed incident report
            MessageBox.Show(reportText, "📋 CARL INCIDENT REPORT", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        /// <summary>
        /// Carl's processor state corruption handler
        /// </summary>
        public static void ShowProcessorCarlified(string processorState)
        {
            var message = $"💾 PROCESSOR STATE CORRUPTED BY CARL 💾\n\n" +
                $"Current State: {processorState}\n\n" +
                "Diagnostic Results:\n" +
                "• All registers flooded with llama interference\n" +
                "• Stack pointer pointing to interdimensional space\n" +
                "• Program counter stuck in infinite Carl loop\n" +
                "• Cache filled with button press patterns\n\n" +
                "Attempting automatic Carl recovery...\n" +
                "(This may take several attempts)";
            
            MessageBox.Show(message, "🔧 CARL PROCESSOR RECOVERY", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        #endregion
    }
}
