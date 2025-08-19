using System;
using System.Management;
using System.Management;
using System.Diagnostics;
using System.Collections.Generic;

namespace ProcessorEmulator.Tools
{
    // NOTE: Add reference to System.Management in your project for hardware probing to work.
    public static class HardwareHealthProbe
    {
        public static Dictionary<string, string> ProbeDiskHealth()
        {
            var results = new Dictionary<string, string>();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject disk in searcher.Get())
                {
                    string model = disk["Model"]?.ToString() ?? "Unknown";
                    string status = disk["Status"]?.ToString() ?? "Unknown";
                    string smartStatus = disk["PredictFailure"]?.ToString() ?? "Unknown";
                    results[model] = $"Status: {status}, SMART: {smartStatus}";
                }
            }
            catch (Exception ex)
            {
                results["Error"] = ex.Message;
            }
            return results;
        }

        public static bool WindowsHasDiskWarning()
        {
            // Check for running Windows disk warning dialogs
            foreach (Process proc in Process.GetProcessesByName("WerFault"))
            {
                if (proc.MainWindowTitle.Contains("hard disk problem") || proc.MainWindowTitle.Contains("disk problem"))
                    return true;
            }
            return false;
        }
    }
}
