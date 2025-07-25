using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace ProcessorEmulator.Emulation.SyncEngine
{
    /// <summary>
    /// Emulates DOCSIS CMTS (Cable Modem Termination System) for cable modem boot sequence
    /// </summary>
    public class CMTSResponder
    {
        private readonly Dictionary<string, CableModem> registeredModems;
        private readonly DOCSISConfig docsisConfig;
        private bool isRunning;
        
        public CMTSResponder()
        {
            registeredModems = new Dictionary<string, CableModem>();
            docsisConfig = CreateDefaultConfig();
            
            Debug.WriteLine("[CMTSResponder] CMTS emulator initialized");
        }
        
        public void StartCMTS()
        {
            if (isRunning) return;
            
            isRunning = true;
            Debug.WriteLine("[CMTSResponder] CMTS services started");
            
            // Simulate CMTS startup
            LogCMTSEvent("CMTS", "Cable Modem Termination System Online");
            LogCMTSEvent("DHCP", "DHCP Server started on 10.1.1.1");
            LogCMTSEvent("TFTP", "TFTP Server started on 10.1.1.1");
            LogCMTSEvent("TOD", "Time of Day Server started");
        }
        
        public void StopCMTS()
        {
            isRunning = false;
            Debug.WriteLine("[CMTSResponder] CMTS services stopped");
        }
        
        public DOCSISBootResponse HandleBootRequest(string macAddress)
        {
            Debug.WriteLine($"[CMTSResponder] Boot request from modem: {macAddress}");
            
            var modem = GetOrCreateModem(macAddress);
            var response = new DOCSISBootResponse();
            
            // Stage 1: DHCP Discovery
            response.DHCPOffer = CreateDHCPOffer(modem);
            LogCMTSEvent("DHCP", $"DHCP Offer sent to {macAddress} → IP: {modem.IPAddress}");
            
            // Stage 2: TFTP Config File
            response.TFTPConfigFile = CreateTFTPConfig(modem);
            LogCMTSEvent("TFTP", $"Config file served to {macAddress}");
            
            // Stage 3: DOCSIS Registration
            response.RegistrationResponse = CreateRegistrationResponse(modem);
            LogCMTSEvent("REG", $"Registration complete for {macAddress}");
            
            // Stage 4: Baseline Privacy (encryption keys)
            response.BPIKeys = CreateBPIKeys(modem);
            LogCMTSEvent("BPI", $"Encryption keys provisioned for {macAddress}");
            
            // Stage 5: Time of Day
            response.TimeOfDay = DateTime.Now;
            LogCMTSEvent("TOD", $"Time sync provided to {macAddress}");
            
            modem.BootStage = DOCSISBootStage.Online;
            modem.LastSeen = DateTime.Now;
            
            Debug.WriteLine($"[CMTSResponder] Boot sequence complete for {macAddress}");
            return response;
        }
        
        private CableModem GetOrCreateModem(string macAddress)
        {
            if (!registeredModems.ContainsKey(macAddress))
            {
                var modem = new CableModem
                {
                    MacAddress = macAddress,
                    IPAddress = GenerateIPAddress(),
                    SerialNumber = $"STB-{macAddress.Replace(":", "").Substring(6)}",
                    Vendor = "ARRIS",
                    Model = "XG1V4",
                    BootStage = DOCSISBootStage.Scanning,
                    FirstSeen = DateTime.Now
                };
                
                registeredModems[macAddress] = modem;
                Debug.WriteLine($"[CMTSResponder] New modem registered: {macAddress} → {modem.IPAddress}");
            }
            
            return registeredModems[macAddress];
        }
        
        private string GenerateIPAddress()
        {
            // Generate IP in 10.1.1.x range
            var lastOctet = 100 + registeredModems.Count;
            return $"10.1.1.{lastOctet}";
        }
        
        private DHCPOffer CreateDHCPOffer(CableModem modem)
        {
            return new DHCPOffer
            {
                ClientIP = modem.IPAddress,
                ServerIP = "10.1.1.1",
                Gateway = "10.1.1.1",
                SubnetMask = "255.255.255.0",
                DNS1 = "8.8.8.8",
                DNS2 = "8.8.4.4",
                TFTPServer = "10.1.1.1",
                ConfigFile = $"{modem.MacAddress.Replace(":", "")}.cfg",
                LeaseTime = 86400 // 24 hours
            };
        }
        
        private byte[] CreateTFTPConfig(CableModem modem)
        {
            // Create DOCSIS config file (simplified TLV format)
            var config = new List<byte>();
            
            // Network Access Control (always allow)
            AddTLV(config, 3, new byte[] { 1 }); // NetworkAccess = 1 (allowed)
            
            // Class of Service
            AddTLV(config, 4, CreateClassOfService());
            
            // Modem Capabilities
            AddTLV(config, 5, CreateModemCapabilities());
            
            // Downstream Service Flow
            AddTLV(config, 24, CreateServiceFlow(true));
            
            // Upstream Service Flow  
            AddTLV(config, 25, CreateServiceFlow(false));
            
            // SNMP Write Access
            AddTLV(config, 11, Encoding.ASCII.GetBytes("private"));
            
            // Software Upgrade
            AddTLV(config, 21, Encoding.ASCII.GetBytes("tftp://10.1.1.1/firmware.bin"));
            
            Debug.WriteLine($"[CMTSResponder] Generated {config.Count} byte DOCSIS config for {modem.MacAddress}");
            return config.ToArray();
        }
        
        private void AddTLV(List<byte> config, byte type, byte[] value)
        {
            config.Add(type);
            config.Add((byte)value.Length);
            config.AddRange(value);
        }
        
        private byte[] CreateClassOfService()
        {
            // High-speed internet class
            var cos = new List<byte>();
            AddTLV(cos, 1, BitConverter.GetBytes(1u)); // Class ID
            AddTLV(cos, 2, BitConverter.GetBytes(100000000u)); // Max rate downstream (100 Mbps)
            AddTLV(cos, 3, BitConverter.GetBytes(10000000u)); // Max rate upstream (10 Mbps)
            return cos.ToArray();
        }
        
        private byte[] CreateModemCapabilities()
        {
            var caps = new List<byte>();
            AddTLV(caps, 1, new byte[] { 1 }); // Concatenation support
            AddTLV(caps, 2, new byte[] { 1 }); // DOCSIS version
            AddTLV(caps, 3, new byte[] { 1 }); // Fragmentation support
            return caps.ToArray();
        }
        
        private byte[] CreateServiceFlow(bool downstream)
        {
            var sf = new List<byte>();
            AddTLV(sf, 1, BitConverter.GetBytes(downstream ? 1u : 2u)); // Service Flow ID
            AddTLV(sf, 2, new byte[] { 1 }); // QoS Parameter Set Type
            AddTLV(sf, 8, BitConverter.GetBytes(downstream ? 50000000u : 5000000u)); // Max sustained rate
            AddTLV(sf, 9, BitConverter.GetBytes(1522u)); // Max traffic burst
            return sf.ToArray();
        }
        
        private DOCSISRegistrationResponse CreateRegistrationResponse(CableModem modem)
        {
            return new DOCSISRegistrationResponse
            {
                Status = "OK",
                ServiceFlowId = 1,
                DownstreamFrequency = 651000000, // 651 MHz (typical cable frequency)
                UpstreamFrequency = 32000000,    // 32 MHz
                DownstreamChannelId = 1,
                UpstreamChannelId = 1,
                ServiceId = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("EMU1"), 0),
                MaxCPE = 16 // Max customer premise equipment
            };
        }
        
        private BPIKeys CreateBPIKeys(CableModem modem)
        {
            // Generate fake encryption keys
            var random = new Random();
            var authKey = new byte[16];
            var tekKey = new byte[16];
            
            random.NextBytes(authKey);
            random.NextBytes(tekKey);
            
            return new BPIKeys
            {
                AuthKey = authKey,
                TEKKey = tekKey,
                KeySequenceNumber = 1,
                ExpiryTime = DateTime.Now.AddDays(7)
            };
        }
        
        private DOCSISConfig CreateDefaultConfig()
        {
            return new DOCSISConfig
            {
                CMTSName = "EMULATOR-CMTS-001",
                SystemName = "ProcessorEmulator DOCSIS Head-End",
                Location = "Virtual Lab Environment",
                Contact = "emulator@localhost",
                DownstreamFrequencies = new uint[] { 651000000, 657000000, 663000000, 669000000 },
                UpstreamFrequencies = new uint[] { 32000000, 38000000, 44000000, 50000000 },
                MaxModems = 1000,
                DHCPPoolStart = "10.1.1.100",
                DHCPPoolEnd = "10.1.1.200"
            };
        }
        
        public List<CableModem> GetRegisteredModems()
        {
            return new List<CableModem>(registeredModems.Values);
        }
        
        public DOCSISConfig GetConfig()
        {
            return docsisConfig;
        }
        
        private void LogCMTSEvent(string service, string message)
        {
            Debug.WriteLine($"[CMTS-{service}] {message}");
        }
        
        public bool IsRunning => isRunning;
    }
    
    // Data models for DOCSIS emulation
    public class CableModem
    {
        public string MacAddress { get; set; }
        public string IPAddress { get; set; }
        public string SerialNumber { get; set; }
        public string Vendor { get; set; }
        public string Model { get; set; }
        public DOCSISBootStage BootStage { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public int SignalStrength { get; set; } = 35; // dBmV
        public double SNR { get; set; } = 40.5; // dB
    }
    
    public enum DOCSISBootStage
    {
        Scanning,
        Ranging,
        DHCP,
        TFTP,
        Registration,
        BPI,
        Online,
        Offline
    }
    
    public class DOCSISBootResponse
    {
        public DHCPOffer DHCPOffer { get; set; }
        public byte[] TFTPConfigFile { get; set; }
        public DOCSISRegistrationResponse RegistrationResponse { get; set; }
        public BPIKeys BPIKeys { get; set; }
        public DateTime TimeOfDay { get; set; }
    }
    
    public class DHCPOffer
    {
        public string ClientIP { get; set; }
        public string ServerIP { get; set; }
        public string Gateway { get; set; }
        public string SubnetMask { get; set; }
        public string DNS1 { get; set; }
        public string DNS2 { get; set; }
        public string TFTPServer { get; set; }
        public string ConfigFile { get; set; }
        public int LeaseTime { get; set; }
    }
    
    public class DOCSISRegistrationResponse
    {
        public string Status { get; set; }
        public uint ServiceFlowId { get; set; }
        public uint DownstreamFrequency { get; set; }
        public uint UpstreamFrequency { get; set; }
        public byte DownstreamChannelId { get; set; }
        public byte UpstreamChannelId { get; set; }
        public uint ServiceId { get; set; }
        public byte MaxCPE { get; set; }
    }
    
    public class BPIKeys
    {
        public byte[] AuthKey { get; set; }
        public byte[] TEKKey { get; set; }
        public uint KeySequenceNumber { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
    
    public class DOCSISConfig
    {
        public string CMTSName { get; set; }
        public string SystemName { get; set; }
        public string Location { get; set; }
        public string Contact { get; set; }
        public uint[] DownstreamFrequencies { get; set; }
        public uint[] UpstreamFrequencies { get; set; }
        public int MaxModems { get; set; }
        public string DHCPPoolStart { get; set; }
        public string DHCPPoolEnd { get; set; }
    }
}
