using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProcessorEmulator.Emulation.SyncEngine
{
    /// <summary>
    /// Maps scraped channels to virtual tuner slots and handles channel virtualization
    /// </summary>
    public class ChannelMapper
    {
        private readonly Dictionary<string, VirtualChannel> channelMap;
        private readonly Dictionary<int, VirtualTuner> tunerSlots;
        private int nextChannelNumber = 100;
        
        public ChannelMapper()
        {
            channelMap = new Dictionary<string, VirtualChannel>();
            tunerSlots = new Dictionary<int, VirtualTuner>();
            
            // Initialize virtual tuners (typical RDK-V box has 6-8 tuners)
            for (int i = 0; i < 8; i++)
            {
                tunerSlots[i] = new VirtualTuner
                {
                    TunerId = i,
                    IsAvailable = true,
                    Frequency = 0,
                    SignalStrength = 85, // Good signal
                    ErrorRate = 0
                };
            }
            
            Debug.WriteLine("[ChannelMapper] Initialized with 8 virtual tuners");
        }
        
        public void MapGuideToChannels(GuideData guideData)
        {
            Debug.WriteLine($"[ChannelMapper] Mapping {guideData.Channels.Count} channels to virtual tuners...");
            
            channelMap.Clear();
            int mappedCount = 0;
            
            foreach (var guideChannel in guideData.Channels)
            {
                var virtualChannel = CreateVirtualChannel(guideChannel);
                channelMap[guideChannel.Id] = virtualChannel;
                mappedCount++;
                
                Debug.WriteLine($"[ChannelMapper] Mapped {guideChannel.Name} → CH{virtualChannel.VirtualNumber} (Freq: {virtualChannel.Frequency})");
            }
            
            Debug.WriteLine($"[ChannelMapper] Channel mapping complete: {mappedCount} channels mapped");
        }
        
        private VirtualChannel CreateVirtualChannel(GuideChannel guideChannel)
        {
            // Parse channel number or assign next available
            int channelNum;
            if (!int.TryParse(guideChannel.Number, out channelNum))
            {
                channelNum = nextChannelNumber++;
            }
            
            // Calculate fake RF frequency (cable TV frequency plan)
            uint frequency = CalculateCableFrequency(channelNum);
            
            return new VirtualChannel
            {
                SourceChannelId = guideChannel.Id,
                VirtualNumber = channelNum,
                CallSign = guideChannel.Name,
                Frequency = frequency,
                ModulationType = "QAM256", // Standard cable modulation
                ServiceId = (ushort)(channelNum + 1000),
                ProgramNumber = (ushort)channelNum,
                StreamUrl = guideChannel.StreamUrl,
                Category = guideChannel.Category,
                IsEncrypted = false, // Free channels
                IsActive = true
            };
        }
        
        private uint CalculateCableFrequency(int channelNumber)
        {
            // Standard cable TV frequency calculation
            if (channelNumber >= 2 && channelNumber <= 4)
            {
                // VHF low band: 54-72 MHz
                return (uint)(54000000 + (channelNumber - 2) * 6000000);
            }
            else if (channelNumber >= 5 && channelNumber <= 6)
            {
                // VHF low band: 76-88 MHz  
                return (uint)(76000000 + (channelNumber - 5) * 6000000);
            }
            else if (channelNumber >= 7 && channelNumber <= 13)
            {
                // VHF high band: 174-216 MHz
                return (uint)(174000000 + (channelNumber - 7) * 6000000);
            }
            else if (channelNumber >= 14 && channelNumber <= 83)
            {
                // UHF band: 470-890 MHz
                return (uint)(470000000 + (channelNumber - 14) * 6000000);
            }
            else
            {
                // Cable channels: start at 91 MHz and increment by 6 MHz
                return (uint)(91000000 + (channelNumber - 100) * 6000000);
            }
        }
        
        public TuneResult TuneToChannel(int channelNumber)
        {
            Debug.WriteLine($"[ChannelMapper] Tuning request for channel {channelNumber}...");
            
            var virtualChannel = channelMap.Values.FirstOrDefault(ch => ch.VirtualNumber == channelNumber);
            if (virtualChannel == null)
            {
                Debug.WriteLine($"[ChannelMapper] Channel {channelNumber} not found");
                return new TuneResult { Success = false, ErrorMessage = "Channel not found" };
            }
            
            // Find available tuner
            var availableTuner = tunerSlots.Values.FirstOrDefault(t => t.IsAvailable);
            if (availableTuner == null)
            {
                Debug.WriteLine($"[ChannelMapper] No available tuners");
                return new TuneResult { Success = false, ErrorMessage = "No tuners available" };
            }
            
            // Simulate tuning
            availableTuner.IsAvailable = false;
            availableTuner.CurrentChannel = virtualChannel;
            availableTuner.Frequency = virtualChannel.Frequency;
            availableTuner.LockTime = DateTime.Now;
            
            Debug.WriteLine($"[ChannelMapper] Tuned tuner {availableTuner.TunerId} to {virtualChannel.CallSign} (Freq: {virtualChannel.Frequency / 1000000.0:F1} MHz)");
            
            return new TuneResult
            {
                Success = true,
                TunerId = availableTuner.TunerId,
                Frequency = virtualChannel.Frequency,
                SignalStrength = availableTuner.SignalStrength,
                Channel = virtualChannel
            };
        }
        
        public void ReleaseTuner(int tunerId)
        {
            if (tunerSlots.ContainsKey(tunerId))
            {
                var tuner = tunerSlots[tunerId];
                Debug.WriteLine($"[ChannelMapper] Releasing tuner {tunerId} from {tuner.CurrentChannel?.CallSign}");
                
                tuner.IsAvailable = true;
                tuner.CurrentChannel = null;
                tuner.Frequency = 0;
            }
        }
        
        public List<VirtualChannel> GetChannelLineup()
        {
            return channelMap.Values.OrderBy(ch => ch.VirtualNumber).ToList();
        }
        
        public List<VirtualTuner> GetTunerStatus()
        {
            return tunerSlots.Values.ToList();
        }
        
        public VirtualChannel GetChannelByNumber(int channelNumber)
        {
            return channelMap.Values.FirstOrDefault(ch => ch.VirtualNumber == channelNumber);
        }
        
        public int GetAvailableTunerCount()
        {
            return tunerSlots.Values.Count(t => t.IsAvailable);
        }
    }
    
    // Data models for channel mapping
    public class VirtualChannel
    {
        public string SourceChannelId { get; set; }
        public int VirtualNumber { get; set; }
        public string CallSign { get; set; }
        public uint Frequency { get; set; }
        public string ModulationType { get; set; }
        public ushort ServiceId { get; set; }
        public ushort ProgramNumber { get; set; }
        public string StreamUrl { get; set; }
        public string Category { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class VirtualTuner
    {
        public int TunerId { get; set; }
        public bool IsAvailable { get; set; }
        public uint Frequency { get; set; }
        public int SignalStrength { get; set; }
        public double ErrorRate { get; set; }
        public VirtualChannel CurrentChannel { get; set; }
        public DateTime LockTime { get; set; }
    }
    
    public class TuneResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int TunerId { get; set; }
        public uint Frequency { get; set; }
        public int SignalStrength { get; set; }
        public VirtualChannel Channel { get; set; }
    }
}
