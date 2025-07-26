using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessorEmulator.CarlContainmentProtocol
{
    /// <summary>
    /// Carl Containment Protocol - Emergency Response System
    /// For when Carl presses buttons he absolutely should not press
    /// Last updated: After the Great Llama Incident of 2025
    /// </summary>
    public static class CarlMonitor
    {
        private static readonly uint CARL_BOOT_ADDRESS = 0xCA71B007;
        private static readonly uint FORBIDDEN_BUTTON = 0xDEADBEEF;
        private static readonly uint LLAMA_PORTAL = 0xBA11ABAD;
        
        private static bool isCarlContained = true;
        private static int llamaSwarmCount = 0;
        private static DateTime lastButtonPress = DateTime.MinValue;
        private static List<string> carlIncidents = new List<string>();
        
        public enum CarlThreatLevel
        {
            Green = 0,      // Carl is contained (probably napping)
            Yellow = 1,     // Carl is looking at buttons suspiciously
            Orange = 2,     // Carl has located the forbidden button
            Red = 3,        // Carl has pressed A button (not necessarily THE button)
            Critical = 4,   // Carl has pressed THE button
            Apocalyptic = 5 // Interdimensional llamas detected
        }
        
        public static CarlThreatLevel CurrentThreatLevel { get; private set; } = CarlThreatLevel.Green;
        
        /// <summary>
        /// Check if Carl has compromised system integrity
        /// </summary>
        public static bool IsCarlEventDetected(uint address, byte[] data)
        {
            // Carl-specific memory patterns
            if (address == CARL_BOOT_ADDRESS)
            {
                LogCarlIncident("🚨 CARL ALERT: Boot address accessed! Initiating containment protocols!");
                CurrentThreatLevel = CarlThreatLevel.Orange;
                return true;
            }
            
            if (address == FORBIDDEN_BUTTON)
            {
                LogCarlIncident("💥 CRITICAL: Carl has found the forbidden button! All hands to battle stations!");
                CurrentThreatLevel = CarlThreatLevel.Critical;
                TriggerCarlEmergencyProtocol();
                return true;
            }
            
            if (address == LLAMA_PORTAL)
            {
                LogCarlIncident("🦙 DIMENSIONAL BREACH: Llama portal detected! Carl has done it again!");
                CurrentThreatLevel = CarlThreatLevel.Apocalyptic;
                HandleLlamaIncursion();
                return true;
            }
            
            // Check for Carl's signature byte patterns
            if (data != null && data.Length >= 4)
            {
                var signature = BitConverter.ToUInt32(data, 0);
                if (signature == 0xCAFFEEED) // Carl's favorite snack code
                {
                    LogCarlIncident("☕ Carl caffeine signature detected. Monitoring for hyperactivity.");
                    CurrentThreatLevel = CarlThreatLevel.Yellow;
                    return true;
                }
            }
            
            return false;
        }
        
        private static void TriggerCarlEmergencyProtocol()
        {
            LogCarlIncident("🚨 INITIATING CARL EMERGENCY PROTOCOL 🚨");
            LogCarlIncident("Step 1: Disconnect all non-essential buttons");
            LogCarlIncident("Step 2: Activate llama-proof barriers");
            LogCarlIncident("Step 3: Hide the good coffee");
            LogCarlIncident("Step 4: Play calming elevator music");
            
            // Simulate system recovery
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                LogCarlIncident("🔧 Emergency protocol complete. Carl contained... for now.");
                CurrentThreatLevel = CarlThreatLevel.Yellow;
            });
        }
        
        private static void HandleLlamaIncursion()
        {
            llamaSwarmCount++;
            LogCarlIncident($"🦙 LLAMA SWARM #{llamaSwarmCount} DETECTED!");
            LogCarlIncident("Deploying emergency hay barriers...");
            LogCarlIncident("Activating dimensional vacuum cleaner...");
            LogCarlIncident("Sending strongly worded memo to Carl...");
            
            if (llamaSwarmCount >= 3)
            {
                LogCarlIncident("💀 MULTIPLE LLAMA INCIDENTS! Considering Carl's permanent vacation to Dimension X!");
            }
        }
        
        private static void LogCarlIncident(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] CARL: {message}";
            carlIncidents.Add(logEntry);
            Debug.WriteLine(logEntry);
            Console.WriteLine(logEntry);
            
            // Keep only last 50 incidents (Carl generates a LOT of logs)
            if (carlIncidents.Count > 50)
            {
                carlIncidents.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Get Carl's incident report for debugging
        /// </summary>
        public static List<string> GetCarlIncidentReport()
        {
            var report = new List<string>
            {
                "=== CARL INCIDENT REPORT ===",
                $"Current Threat Level: {CurrentThreatLevel}",
                $"Total Llama Incidents: {llamaSwarmCount}",
                $"Last Button Press: {(lastButtonPress == DateTime.MinValue ? "Never (suspicious)" : lastButtonPress.ToString())}",
                $"Carl Containment Status: {(isCarlContained ? "Contained (probably)" : "AT LARGE!")}",
                "",
                "Recent Incidents:"
            };
            
            report.AddRange(carlIncidents);
            report.Add("=== END REPORT ===");
            
            return report;
        }
        
        /// <summary>
        /// Carl-safe register write with built-in chaos detection
        /// </summary>
        public static bool SafeWriteRegister(uint address, byte[] data, Action<uint, byte[]> actualWrite)
        {
            // Pre-emptive Carl detection
            if (IsCarlEventDetected(address, data))
            {
                LogCarlIncident($"⚠️ Blocked potentially Carl-influenced write to 0x{address:X8}");
                return false;
            }
            
            try
            {
                actualWrite(address, data);
                return true;
            }
            catch (LlamaException ex)
            {
                LogCarlIncident($"🦙 LlamaException caught: {ex.Message}");
                LogCarlIncident("Attempting llama recovery procedures...");
                return false;
            }
            catch (Exception ex) when (ex.Message.Contains("Carl"))
            {
                LogCarlIncident($"💥 Carl-related exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Reset Carl containment after successful intervention
        /// </summary>
        public static void ResetCarlContainment()
        {
            LogCarlIncident("🔄 Resetting Carl containment protocols...");
            CurrentThreatLevel = CarlThreatLevel.Green;
            isCarlContained = true;
            LogCarlIncident("✅ Carl containment reset. All clear... until next time.");
        }
    }
    
    /// <summary>
    /// Carl-specific exceptions for when things go interdimensionally wrong
    /// </summary>
    public class LlamaException : Exception
    {
        public LlamaException(string message) : base($"🦙 LLAMA ALERT: {message}") { }
        public LlamaException(string message, Exception innerException) 
            : base($"🦙 LLAMA ALERT: {message}", innerException) { }
    }
    
    public class CarlException : Exception
    {
        public CarlException(string message) : base($"🔴 CARL INCIDENT: {message}") { }
        public CarlException(string message, Exception innerException) 
            : base($"🔴 CARL INCIDENT: {message}", innerException) { }
    }
    
    /// <summary>
    /// Emergency recovery modes for Carl-induced chaos
    /// </summary>
    public enum LlamaRecoveryMode
    {
        Ignore,              // Pretend it's not happening (not recommended)
        ContainAndContinue,  // Contain the llamas, continue operation
        FullSystemReboot,    // Nuclear option: restart everything
        CallForBackup,       // Alert all available personnel
        HideUntilItStops     // Sometimes this actually works
    }
}
