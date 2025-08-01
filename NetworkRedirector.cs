using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ProcessorEmulator
{
    /// <summary>
    /// Network redirector for spoofing Comcast DNS endpoints
    /// Redirects xcal.tv and xconf.comcast.net to local service emulator
    /// </summary>
    public class NetworkRedirector
    {
        #region Configuration
        
        private readonly Dictionary<string, string> dnsRedirects = new()
        {
            ["xcal.tv"] = "127.0.0.1",
            ["xconf.comcast.net"] = "127.0.0.1",
            ["comcast.net"] = "127.0.0.1",
            ["xfinity.com"] = "127.0.0.1",
            ["x1platform.comcast.com"] = "127.0.0.1",
            ["xtv.comcast.net"] = "127.0.0.1"
        };
        
        private const string HOSTS_FILE_PATH = @"C:\Windows\System32\drivers\etc\hosts";
        private const string BACKUP_SUFFIX = ".processor-emulator-backup";
        
        #endregion

        #region Fields
        
        private bool redirectsActive;
        private List<string> originalHostsContent;
        private Process dnsmasqProcess;
        
        #endregion

        #region Setup and Teardown
        
        public async Task<bool> Setup()
        {
            Console.WriteLine("🔀 Setting up network redirection...");
            
            try
            {
                // Method 1: Try Windows hosts file modification
                if (await SetupHostsFileRedirection())
                {
                    Console.WriteLine("✅ DNS redirection via hosts file successful");
                    redirectsActive = true;
                    return true;
                }
                
                // Method 2: Try dnsmasq installation (if available)
                if (await SetupDnsmasqRedirection())
                {
                    Console.WriteLine("✅ DNS redirection via dnsmasq successful");
                    redirectsActive = true;
                    return true;
                }
                
                // Method 3: Manual instructions
                await ProvideManualInstructions();
                
                return true; // Continue even if automatic setup fails
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Network redirection setup failed: {ex.Message}");
                await ProvideManualInstructions();
                return false;
            }
        }
        
        private async Task<bool> SetupHostsFileRedirection()
        {
            try
            {
                Console.WriteLine("🔧 Configuring Windows hosts file...");
                
                // Check if we have admin privileges
                if (!IsRunningAsAdmin())
                {
                    Console.WriteLine("⚠️ Administrator privileges required for hosts file modification");
                    return false;
                }
                
                // Backup original hosts file
                await BackupHostsFile();
                
                // Read current hosts file
                var hostsContent = new List<string>();
                if (File.Exists(HOSTS_FILE_PATH))
                {
                    hostsContent.AddRange(await File.ReadAllLinesAsync(HOSTS_FILE_PATH));
                }
                
                // Add our redirections
                hostsContent.Add("");
                hostsContent.Add("# Processor Emulator - Comcast Service Redirects");
                hostsContent.Add($"# Added on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                
                foreach (var redirect in dnsRedirects)
                {
                    hostsContent.Add($"{redirect.Value}\t{redirect.Key}");
                    Console.WriteLine($"Added redirect: {redirect.Key} -> {redirect.Value}");
                }
                
                hostsContent.Add("# End Processor Emulator redirects");
                
                // Write updated hosts file
                await File.WriteAllLinesAsync(HOSTS_FILE_PATH, hostsContent);
                
                // Flush DNS cache
                await FlushDnsCache();
                
                Console.WriteLine("✅ Hosts file redirection configured");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hosts file setup failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task<bool> SetupDnsmasqRedirection()
        {
            try
            {
                Console.WriteLine("🔧 Setting up dnsmasq redirection...");
                
                // Check if dnsmasq is available
                var dnsmasqPath = await FindDnsmasq();
                if (string.IsNullOrEmpty(dnsmasqPath))
                {
                    Console.WriteLine("❌ dnsmasq not found, skipping");
                    return false;
                }
                
                // Create dnsmasq configuration
                var configPath = Path.Combine(Path.GetTempPath(), "processor-emulator-dnsmasq.conf");
                var configLines = new List<string>
                {
                    "# Processor Emulator DNS Configuration",
                    "port=53",
                    "listen-address=127.0.0.1",
                    "bind-interfaces",
                    ""
                };
                
                foreach (var redirect in dnsRedirects)
                {
                    configLines.Add($"address=/{redirect.Key}/{redirect.Value}");
                }
                
                await File.WriteAllLinesAsync(configPath, configLines);
                
                // Start dnsmasq
                var startInfo = new ProcessStartInfo
                {
                    FileName = dnsmasqPath,
                    Arguments = $"-C {configPath} --log-facility=-",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                dnsmasqProcess = Process.Start(startInfo);
                
                if (dnsmasqProcess != null && !dnsmasqProcess.HasExited)
                {
                    Console.WriteLine($"✅ dnsmasq started (PID: {dnsmasqProcess.Id})");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ dnsmasq setup failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task ProvideManualInstructions()
        {
            Console.WriteLine("\n📋 Manual DNS Setup Instructions:");
            Console.WriteLine("Since automatic DNS redirection failed, please manually configure DNS:");
            Console.WriteLine();
            Console.WriteLine("Option 1 - Edit hosts file as Administrator:");
            Console.WriteLine($"1. Open Notepad as Administrator");
            Console.WriteLine($"2. Open {HOSTS_FILE_PATH}");
            Console.WriteLine("3. Add these lines:");
            
            foreach (var redirect in dnsRedirects)
            {
                Console.WriteLine($"   {redirect.Value}\t{redirect.Key}");
            }
            
            Console.WriteLine("4. Save and close");
            Console.WriteLine("5. Run 'ipconfig /flushdns' in Command Prompt");
            Console.WriteLine();
            Console.WriteLine("Option 2 - Use router DNS override:");
            Console.WriteLine("1. Access your router's admin panel");
            Console.WriteLine("2. Set custom DNS entries for the domains above");
            Console.WriteLine();
            Console.WriteLine("Option 3 - Use third-party DNS tools:");
            Console.WriteLine("1. Install tools like 'Acrylic DNS Proxy' or 'DNS Angel'");
            Console.WriteLine("2. Configure redirects for Comcast domains");
            
            await Task.CompletedTask;
        }
        
        #endregion

        #region Utility Methods
        
        private bool IsRunningAsAdmin()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
        
        private async Task BackupHostsFile()
        {
            if (File.Exists(HOSTS_FILE_PATH))
            {
                var backupPath = HOSTS_FILE_PATH + BACKUP_SUFFIX;
                if (!File.Exists(backupPath))
                {
                    await File.Copy(HOSTS_FILE_PATH, backupPath, true);
                    Console.WriteLine($"📁 Backed up hosts file to {backupPath}");
                }
            }
        }
        
        private async Task<string> FindDnsmasq()
        {
            var possiblePaths = new[]
            {
                "dnsmasq.exe",
                @"C:\Program Files\dnsmasq\dnsmasq.exe",
                @"C:\dnsmasq\dnsmasq.exe",
                @"C:\msys64\mingw64\bin\dnsmasq.exe",
                @"C:\tools\dnsmasq\dnsmasq.exe"
            };
            
            foreach (var path in possiblePaths)
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "where",
                            Arguments = path.Contains("\\") ? $"\"{path}\"" : path,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    
                    process.Start();
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                    {
                        return output.Trim().Split('\n')[0].Trim();
                    }
                }
                catch
                {
                    // Continue to next path
                }
                
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            return null;
        }
        
        private async Task FlushDnsCache()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ipconfig",
                        Arguments = "/flushdns",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                await process.WaitForExitAsync();
                
                Console.WriteLine("🔄 DNS cache flushed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ DNS cache flush failed: {ex.Message}");
            }
        }
        
        #endregion

        #region Cleanup
        
        public async Task<bool> Stop()
        {
            Console.WriteLine("🛑 Cleaning up network redirection...");
            
            try
            {
                // Stop dnsmasq if running
                if (dnsmasqProcess != null && !dnsmasqProcess.HasExited)
                {
                    dnsmasqProcess.Kill();
                    await dnsmasqProcess.WaitForExitAsync();
                    Console.WriteLine("✅ dnsmasq stopped");
                }
                
                // Restore hosts file if we modified it
                if (redirectsActive && IsRunningAsAdmin())
                {
                    await RestoreHostsFile();
                }
                
                redirectsActive = false;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Cleanup failed: {ex.Message}");
                return false;
            }
        }
        
        private async Task RestoreHostsFile()
        {
            try
            {
                var backupPath = HOSTS_FILE_PATH + BACKUP_SUFFIX;
                
                if (File.Exists(backupPath))
                {
                    await File.Copy(backupPath, HOSTS_FILE_PATH, true);
                    File.Delete(backupPath);
                    Console.WriteLine("✅ Hosts file restored from backup");
                    
                    await FlushDnsCache();
                }
                else
                {
                    // Manual cleanup of our entries
                    await RemoveOurHostsEntries();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hosts file restoration failed: {ex.Message}");
                Console.WriteLine("Please manually remove Processor Emulator entries from hosts file");
            }
        }
        
        private async Task RemoveOurHostsEntries()
        {
            if (!File.Exists(HOSTS_FILE_PATH))
                return;
            
            var lines = await File.ReadAllLinesAsync(HOSTS_FILE_PATH);
            var cleanedLines = new List<string>();
            bool inOurSection = false;
            
            foreach (var line in lines)
            {
                if (line.Contains("# Processor Emulator"))
                {
                    inOurSection = true;
                    continue;
                }
                
                if (inOurSection && line.Contains("# End Processor Emulator"))
                {
                    inOurSection = false;
                    continue;
                }
                
                if (!inOurSection)
                {
                    cleanedLines.Add(line);
                }
            }
            
            await File.WriteAllLinesAsync(HOSTS_FILE_PATH, cleanedLines);
            Console.WriteLine("✅ Removed Processor Emulator entries from hosts file");
        }
        
        #endregion

        #region Status and Validation
        
        public bool IsActive => redirectsActive;
        
        public async Task<bool> ValidateRedirection()
        {
            Console.WriteLine("🔍 Validating DNS redirection...");
            
            bool allWorking = true;
            
            foreach (var domain in dnsRedirects.Keys)
            {
                try
                {
                    var addresses = await Dns.GetHostAddressesAsync(domain);
                    var resolved = addresses[0].ToString();
                    
                    if (resolved == dnsRedirects[domain] || resolved == "127.0.0.1")
                    {
                        Console.WriteLine($"✅ {domain} -> {resolved}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ {domain} -> {resolved} (expected {dnsRedirects[domain]})");
                        allWorking = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {domain} -> DNS resolution failed: {ex.Message}");
                    allWorking = false;
                }
            }
            
            return allWorking;
        }
        
        public Dictionary<string, string> GetRedirects()
        {
            return new Dictionary<string, string>(dnsRedirects);
        }
        
        #endregion
    }
}
