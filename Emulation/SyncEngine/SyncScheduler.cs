using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessorEmulator.Emulation.SyncEngine
{
    /// <summary>
    /// Orchestrates all sync operations and manages timing for the virtual STB ecosystem
    /// </summary>
    public class SyncScheduler
    {
        private readonly GuideFetcher guideFetcher;
        private readonly ChannelMapper channelMapper;
        private readonly EntitlementManager entitlementManager;
        private readonly CMTSResponder cmtsResponder;
        
        private readonly Timer syncTimer;
        private readonly List<SyncEvent> syncHistory;
        private bool isRunning;
        private CancellationTokenSource cancellationTokenSource;
        
        public event Action<SyncEvent> SyncEventOccurred;
        
        public SyncScheduler()
        {
            guideFetcher = new GuideFetcher();
            channelMapper = new ChannelMapper();
            entitlementManager = new EntitlementManager();
            cmtsResponder = new CMTSResponder();
            
            syncHistory = new List<SyncEvent>();
            cancellationTokenSource = new CancellationTokenSource();
            
            // Start sync timer (runs every 30 minutes)
            syncTimer = new Timer(OnSyncTimer, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            
            Debug.WriteLine("[SyncScheduler] Sync scheduler initialized");
        }
        
        public async Task StartAsync()
        {
            if (isRunning) return;
            
            isRunning = true;
            cmtsResponder.StartCMTS();
            
            LogSyncEvent("System", "Sync scheduler started", SyncStatus.Success);
            
            // Perform initial sync
            await PerformFullSyncAsync();
            
            Debug.WriteLine("[SyncScheduler] Sync scheduler running");
        }
        
        public void Stop()
        {
            if (!isRunning) return;
            
            isRunning = false;
            cmtsResponder.StopCMTS();
            cancellationTokenSource.Cancel();
            
            LogSyncEvent("System", "Sync scheduler stopped", SyncStatus.Success);
            Debug.WriteLine("[SyncScheduler] Sync scheduler stopped");
        }
        
        private async void OnSyncTimer(object state)
        {
            if (!isRunning) return;
            
            try
            {
                await PerformScheduledSyncAsync();
            }
            catch (Exception ex)
            {
                LogSyncEvent("Timer", $"Scheduled sync failed: {ex.Message}", SyncStatus.Failed);
                Debug.WriteLine($"[SyncScheduler] Timer sync error: {ex.Message}");
            }
        }
        
        public async Task PerformFullSyncAsync()
        {
            Debug.WriteLine("[SyncScheduler] Starting full sync...");
            var startTime = DateTime.Now;
            
            try
            {
                // Step 1: Fetch guide data
                LogSyncEvent("Guide", "Fetching TV guide data...", SyncStatus.InProgress);
                var guideData = await guideFetcher.FetchGuideAsync();
                LogSyncEvent("Guide", $"Guide updated: {guideData.Channels.Count} channels", SyncStatus.Success);
                
                // Step 2: Map channels to virtual tuners
                LogSyncEvent("Channels", "Mapping channels to virtual tuners...", SyncStatus.InProgress);
                channelMapper.MapGuideToChannels(guideData);
                var channelCount = channelMapper.GetChannelLineup().Count;
                LogSyncEvent("Channels", $"Channel mapping complete: {channelCount} channels mapped", SyncStatus.Success);
                
                // Step 3: Verify entitlements
                LogSyncEvent("Entitlements", "Verifying service entitlements...", SyncStatus.InProgress);
                var activation = entitlementManager.GetBoxActivation();
                var entitlementCount = entitlementManager.GetAllEntitlements().Count;
                LogSyncEvent("Entitlements", $"Entitlements verified: {entitlementCount} services, box {(activation.IsActivated ? "activated" : "not activated")}", 
                           activation.IsActivated ? SyncStatus.Success : SyncStatus.Warning);
                
                // Step 4: CMTS health check
                LogSyncEvent("CMTS", "Checking DOCSIS infrastructure...", SyncStatus.InProgress);
                var registeredModems = cmtsResponder.GetRegisteredModems().Count;
                LogSyncEvent("CMTS", $"CMTS online: {registeredModems} modems registered", SyncStatus.Success);
                
                var duration = DateTime.Now - startTime;
                LogSyncEvent("System", $"Full sync completed in {duration.TotalSeconds:F1}s", SyncStatus.Success);
                
                Debug.WriteLine($"[SyncScheduler] Full sync completed in {duration.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                LogSyncEvent("System", $"Full sync failed: {ex.Message}", SyncStatus.Failed);
                Debug.WriteLine($"[SyncScheduler] Full sync error: {ex.Message}");
            }
        }
        
        private async Task PerformScheduledSyncAsync()
        {
            Debug.WriteLine("[SyncScheduler] Starting scheduled sync...");
            
            // Only sync guide if it's stale
            if (guideFetcher.NeedsSync)
            {
                LogSyncEvent("Guide", "Refreshing stale guide data...", SyncStatus.InProgress);
                var guideData = await guideFetcher.FetchGuideAsync();
                channelMapper.MapGuideToChannels(guideData);
                LogSyncEvent("Guide", $"Guide refreshed: {guideData.Channels.Count} channels", SyncStatus.Success);
            }
            else
            {
                LogSyncEvent("Guide", "Guide data is current", SyncStatus.Success);
            }
            
            // Health check on other services
            var tunerStatus = channelMapper.GetTunerStatus();
            var availableTuners = channelMapper.GetAvailableTunerCount();
            LogSyncEvent("Tuners", $"Tuner status: {availableTuners}/{tunerStatus.Count} available", SyncStatus.Success);
        }
        
        public async Task TriggerManualSyncAsync(string syncType = "full")
        {
            LogSyncEvent("Manual", $"Manual {syncType} sync triggered", SyncStatus.InProgress);
            
            switch (syncType.ToLower())
            {
                case "guide":
                    var guideData = await guideFetcher.FetchGuideAsync();
                    channelMapper.MapGuideToChannels(guideData);
                    LogSyncEvent("Manual", "Guide sync completed", SyncStatus.Success);
                    break;
                    
                case "entitlements":
                    // Force refresh entitlements (in real system would contact headend)
                    LogSyncEvent("Manual", "Entitlement refresh completed", SyncStatus.Success);
                    break;
                    
                case "full":
                default:
                    await PerformFullSyncAsync();
                    break;
            }
        }
        
        public SyncStatus GetOverallStatus()
        {
            var recentEvents = GetRecentSyncEvents(TimeSpan.FromHours(1));
            
            foreach (var evt in recentEvents)
            {
                if (evt.Status == SyncStatus.Failed)
                    return SyncStatus.Failed;
            }
            
            foreach (var evt in recentEvents)
            {
                if (evt.Status == SyncStatus.Warning)
                    return SyncStatus.Warning;
            }
            
            return SyncStatus.Success;
        }
        
        public List<SyncEvent> GetRecentSyncEvents(TimeSpan timespan)
        {
            var cutoff = DateTime.Now - timespan;
            var recent = new List<SyncEvent>();
            
            foreach (var evt in syncHistory)
            {
                if (evt.Timestamp >= cutoff)
                    recent.Add(evt);
            }
            
            return recent;
        }
        
        public List<SyncEvent> GetAllSyncEvents()
        {
            return new List<SyncEvent>(syncHistory);
        }
        
        private void LogSyncEvent(string component, string message, SyncStatus status)
        {
            var syncEvent = new SyncEvent
            {
                Timestamp = DateTime.Now,
                Component = component,
                Message = message,
                Status = status
            };
            
            syncHistory.Add(syncEvent);
            
            // Keep history manageable (last 1000 events)
            if (syncHistory.Count > 1000)
            {
                syncHistory.RemoveAt(0);
            }
            
            // Notify listeners
            SyncEventOccurred?.Invoke(syncEvent);
            
            Debug.WriteLine($"[Sync-{component}] {message} ({status})");
        }
        
        // Public accessors for other components
        public GuideFetcher GuideFetcher => guideFetcher;
        public ChannelMapper ChannelMapper => channelMapper;
        public EntitlementManager EntitlementManager => entitlementManager;
        public CMTSResponder CMTSResponder => cmtsResponder;
        
        public bool IsRunning => isRunning;
        public DateTime LastSyncTime => syncHistory.Count > 0 ? syncHistory[^1].Timestamp : DateTime.MinValue;
        
        public void Dispose()
        {
            Stop();
            syncTimer?.Dispose();
            cancellationTokenSource?.Dispose();
        }
    }
    
    // Data models for sync events
    public class SyncEvent
    {
        public DateTime Timestamp { get; set; }
        public string Component { get; set; }
        public string Message { get; set; }
        public SyncStatus Status { get; set; }
    }
    
    public enum SyncStatus
    {
        InProgress,
        Success,
        Warning,
        Failed
    }
}
