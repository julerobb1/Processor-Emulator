using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ProcessorEmulator.Emulation.SyncEngine
{
    /// <summary>
    /// Manages entitlements and authorization for virtual STB - makes firmware think box is activated
    /// </summary>
    public class EntitlementManager
    {
        private readonly Dictionary<string, ServiceEntitlement> entitlements;
        private readonly string entitlementCacheFile;
        private BoxActivation boxActivation;
        
        public EntitlementManager()
        {
            entitlements = new Dictionary<string, ServiceEntitlement>();
            
            var cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessorEmulator", "Entitlements");
            Directory.CreateDirectory(cacheDir);
            entitlementCacheFile = Path.Combine(cacheDir, "entitlements.json");
            
            InitializeDefaultEntitlements();
            LoadCachedEntitlements();
            
            Debug.WriteLine("[EntitlementManager] Initialized with default entitlements");
        }
        
        private void InitializeDefaultEntitlements()
        {
            // Create box activation record
            boxActivation = new BoxActivation
            {
                SerialNumber = "ARRIS-XG1V4-EMU001",
                MacAddress = "00:1A:2B:3C:4D:5E",
                IsActivated = true,
                ActivationDate = DateTime.Now.AddMonths(-6), // Activated 6 months ago
                AccountNumber = "123456789012",
                ServiceTier = "Premium",
                Region = "USA-East",
                HeadendId = "EMULATOR_HE_001"
            };
            
            // Add basic service entitlements
            AddServiceEntitlement("BASIC_TV", "Basic TV Service", true, DateTime.Now.AddYears(1));
            AddServiceEntitlement("PREMIUM_TV", "Premium TV Channels", true, DateTime.Now.AddYears(1));
            AddServiceEntitlement("DVR_SERVICE", "DVR Recording", true, DateTime.Now.AddYears(1));
            AddServiceEntitlement("ON_DEMAND", "Video On Demand", true, DateTime.Now.AddYears(1));
            AddServiceEntitlement("WHOLE_HOME", "Whole Home DVR", true, DateTime.Now.AddYears(1));
            
            // Add channel-specific entitlements
            for (int ch = 2; ch <= 999; ch++)
            {
                AddChannelEntitlement(ch, true);
            }
            
            Debug.WriteLine($"[EntitlementManager] Created {entitlements.Count} default entitlements");
        }
        
        private void AddServiceEntitlement(string serviceId, string serviceName, bool authorized, DateTime expiryDate)
        {
            entitlements[serviceId] = new ServiceEntitlement
            {
                ServiceId = serviceId,
                ServiceName = serviceName,
                IsAuthorized = authorized,
                ExpiryDate = expiryDate,
                EntitlementType = EntitlementType.Service
            };
        }
        
        private void AddChannelEntitlement(int channelNumber, bool authorized)
        {
            var channelId = $"CH_{channelNumber:D3}";
            entitlements[channelId] = new ServiceEntitlement
            {
                ServiceId = channelId,
                ServiceName = $"Channel {channelNumber}",
                IsAuthorized = authorized,
                ExpiryDate = DateTime.Now.AddYears(1),
                EntitlementType = EntitlementType.Channel,
                ChannelNumber = channelNumber
            };
        }
        
        public AuthorizationResponse CheckAuthorization(string serviceId)
        {
            Debug.WriteLine($"[EntitlementManager] Authorization check for: {serviceId}");
            
            if (!boxActivation.IsActivated)
            {
                return new AuthorizationResponse
                {
                    IsAuthorized = false,
                    ReasonCode = "BOX_NOT_ACTIVATED",
                    Message = "Set-top box is not activated"
                };
            }
            
            if (entitlements.ContainsKey(serviceId))
            {
                var entitlement = entitlements[serviceId];
                
                if (!entitlement.IsAuthorized)
                {
                    return new AuthorizationResponse
                    {
                        IsAuthorized = false,
                        ReasonCode = "SERVICE_NOT_SUBSCRIBED",
                        Message = $"Not subscribed to {entitlement.ServiceName}"
                    };
                }
                
                if (entitlement.ExpiryDate < DateTime.Now)
                {
                    return new AuthorizationResponse
                    {
                        IsAuthorized = false,
                        ReasonCode = "SUBSCRIPTION_EXPIRED",
                        Message = $"Subscription to {entitlement.ServiceName} has expired"
                    };
                }
                
                return new AuthorizationResponse
                {
                    IsAuthorized = true,
                    ReasonCode = "AUTHORIZED",
                    Message = $"Authorized for {entitlement.ServiceName}",
                    ExpiryDate = entitlement.ExpiryDate
                };
            }
            
            // Default to authorized for unknown services (permissive emulation)
            return new AuthorizationResponse
            {
                IsAuthorized = true,
                ReasonCode = "DEFAULT_AUTHORIZED",
                Message = "Default authorization granted"
            };
        }
        
        public AuthorizationResponse CheckChannelAuthorization(int channelNumber)
        {
            var channelId = $"CH_{channelNumber:D3}";
            return CheckAuthorization(channelId);
        }
        
        public BoxActivation GetBoxActivation()
        {
            return boxActivation;
        }
        
        public void SetBoxActivation(bool activated)
        {
            boxActivation.IsActivated = activated;
            if (activated && boxActivation.ActivationDate == DateTime.MinValue)
            {
                boxActivation.ActivationDate = DateTime.Now;
            }
            
            Debug.WriteLine($"[EntitlementManager] Box activation set to: {activated}");
            SaveEntitlements();
        }
        
        public void UpdateServiceEntitlement(string serviceId, bool authorized, DateTime? expiryDate = null)
        {
            if (entitlements.ContainsKey(serviceId))
            {
                entitlements[serviceId].IsAuthorized = authorized;
                if (expiryDate.HasValue)
                {
                    entitlements[serviceId].ExpiryDate = expiryDate.Value;
                }
                
                Debug.WriteLine($"[EntitlementManager] Updated entitlement {serviceId}: authorized={authorized}");
                SaveEntitlements();
            }
        }
        
        public List<ServiceEntitlement> GetAllEntitlements()
        {
            return new List<ServiceEntitlement>(entitlements.Values);
        }
        
        public byte[] GenerateEntitlementTLV()
        {
            // Generate fake TLV (Type-Length-Value) entitlement message like real cable boxes use
            var tlvData = new List<byte>();
            
            // TLV Header
            tlvData.AddRange(Encoding.ASCII.GetBytes("ENT1")); // Magic
            tlvData.AddRange(BitConverter.GetBytes((uint)0x12345678)); // Timestamp
            
            // Box ID TLV
            tlvData.Add(0x01); // Type: Box ID
            tlvData.Add(0x10); // Length: 16 bytes
            tlvData.AddRange(Encoding.ASCII.GetBytes(boxActivation.SerialNumber.PadRight(16)));
            
            // Activation Status TLV
            tlvData.Add(0x02); // Type: Activation
            tlvData.Add(0x01); // Length: 1 byte
            tlvData.Add(boxActivation.IsActivated ? (byte)0x01 : (byte)0x00);
            
            // Service Tier TLV
            tlvData.Add(0x03); // Type: Service Tier
            tlvData.Add(0x04); // Length: 4 bytes
            tlvData.AddRange(BitConverter.GetBytes(GetServiceTierCode()));
            
            // Channel Bitmap TLV (simplified - just say all channels authorized)
            tlvData.Add(0x04); // Type: Channel bitmap
            tlvData.Add(0x80); // Length: 128 bytes (1024 channels / 8 bits)
            for (int i = 0; i < 128; i++)
            {
                tlvData.Add(0xFF); // All channels authorized
            }
            
            Debug.WriteLine($"[EntitlementManager] Generated {tlvData.Count} byte TLV entitlement message");
            return tlvData.ToArray();
        }
        
        private uint GetServiceTierCode()
        {
            return boxActivation.ServiceTier switch
            {
                "Basic" => 0x1000,
                "Standard" => 0x2000,
                "Premium" => 0x3000,
                _ => 0x3000
            };
        }
        
        private void SaveEntitlements()
        {
            try
            {
                var data = new EntitlementData
                {
                    BoxActivation = boxActivation,
                    Entitlements = new List<ServiceEntitlement>(entitlements.Values),
                    LastUpdated = DateTime.Now
                };
                
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(entitlementCacheFile, json);
                
                Debug.WriteLine($"[EntitlementManager] Entitlements saved to cache");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EntitlementManager] Failed to save entitlements: {ex.Message}");
            }
        }
        
        private void LoadCachedEntitlements()
        {
            try
            {
                if (File.Exists(entitlementCacheFile))
                {
                    var json = File.ReadAllText(entitlementCacheFile);
                    var data = JsonSerializer.Deserialize<EntitlementData>(json);
                    
                    if (data != null)
                    {
                        boxActivation = data.BoxActivation ?? boxActivation;
                        
                        entitlements.Clear();
                        foreach (var ent in data.Entitlements)
                        {
                            entitlements[ent.ServiceId] = ent;
                        }
                        
                        Debug.WriteLine($"[EntitlementManager] Loaded {entitlements.Count} cached entitlements");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EntitlementManager] Failed to load cached entitlements: {ex.Message}");
            }
        }
    }
    
    // Data models
    public class ServiceEntitlement
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public bool IsAuthorized { get; set; }
        public DateTime ExpiryDate { get; set; }
        public EntitlementType EntitlementType { get; set; }
        public int? ChannelNumber { get; set; }
    }
    
    public enum EntitlementType
    {
        Service,
        Channel,
        Package
    }
    
    public class BoxActivation
    {
        public string SerialNumber { get; set; }
        public string MacAddress { get; set; }
        public bool IsActivated { get; set; }
        public DateTime ActivationDate { get; set; }
        public string AccountNumber { get; set; }
        public string ServiceTier { get; set; }
        public string Region { get; set; }
        public string HeadendId { get; set; }
    }
    
    public class AuthorizationResponse
    {
        public bool IsAuthorized { get; set; }
        public string ReasonCode { get; set; }
        public string Message { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
    
    public class EntitlementData
    {
        public BoxActivation BoxActivation { get; set; }
        public List<ServiceEntitlement> Entitlements { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
