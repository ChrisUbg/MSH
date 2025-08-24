using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MSH.Web.Services;
using MSH.Infrastructure.Interfaces;

namespace MSH.Web.Services
{
    public class DeviceStateInfo
    {
        public string NodeId { get; set; } = string.Empty;
        public string PowerState { get; set; } = "unknown";
        public bool IsOnline { get; set; } = false;
        public decimal? PowerConsumption { get; set; }
        public decimal? Voltage { get; set; }
        public decimal? Current { get; set; }
        public decimal? EnergyToday { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        public string? LastError { get; set; }
    }

    public class MaintainStatiModel
    {
        private readonly ConcurrentDictionary<string, DeviceStateInfo> _deviceStates = new();
        private readonly ILogger<MaintainStatiModel> _logger;
        private readonly IMatterDeviceControlService _matterService;
        private readonly IDeviceEventService? _eventService;
        private readonly SemaphoreSlim _updateSemaphore = new(1, 1);

        // Events for real-time updates
        public event Func<string, DeviceStateInfo, Task>? OnDeviceStateChanged;
        public event Func<string, string, Task>? OnDeviceError;

        public MaintainStatiModel(
            ILogger<MaintainStatiModel> logger,
            IMatterDeviceControlService matterService,
            IDeviceEventService? eventService = null)
        {
            _logger = logger;
            _matterService = matterService;
            _eventService = eventService;
            
            _logger.LogInformation("MaintainStatiModel initialized");
        }

        /// <summary>
        /// Get device state from cache or fetch from device
        /// </summary>
        public async Task<DeviceStateInfo> GetDeviceStateAsync(string nodeId, bool forceRefresh = false)
        {
            try
            {
                // Check if we have a cached state and it's recent enough
                if (!forceRefresh && _deviceStates.TryGetValue(nodeId, out var cachedState))
                {
                    var cacheAge = DateTime.UtcNow - cachedState.LastUpdated;
                    if (cacheAge.TotalSeconds < 60) // Cache for 60 seconds (increased)
                    {
                        _logger.LogDebug("Returning cached state for device {NodeId}, age: {Age}s", nodeId, cacheAge.TotalSeconds);
                        return cachedState;
                    }
                }

                // For initial load, return cached state immediately if available
                if (_deviceStates.TryGetValue(nodeId, out var existingState))
                {
                    _logger.LogInformation("Returning existing cached state for device {NodeId} during load", nodeId);
                    return existingState;
                }

                // Only fetch fresh state if explicitly requested or no cache exists
                if (forceRefresh)
                {
                    return await RefreshDeviceStateAsync(nodeId);
                }
                else
                {
                    // Return a basic state for initial load
                    return new DeviceStateInfo
                    {
                        NodeId = nodeId,
                        PowerState = "unknown",
                        IsOnline = false,
                        LastUpdated = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device state for {NodeId}", nodeId);
                
                // Return cached state if available, otherwise create error state
                if (_deviceStates.TryGetValue(nodeId, out var cachedState))
                {
                    cachedState.LastError = ex.Message;
                    return cachedState;
                }

                return new DeviceStateInfo
                {
                    NodeId = nodeId,
                    PowerState = "unknown",
                    IsOnline = false,
                    LastError = ex.Message,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Refresh device state from the actual device
        /// </summary>
        public async Task<DeviceStateInfo> RefreshDeviceStateAsync(string nodeId)
        {
            await _updateSemaphore.WaitAsync();
            try
            {
                _logger.LogInformation("Refreshing device state for {NodeId}", nodeId);

                // Get current state and online status in parallel
                var stateTask = _matterService.GetDeviceStateAsync(nodeId);
                var onlineTask = _matterService.IsDeviceOnlineAsync(nodeId);
                var powerTask = _matterService.GetPowerMetricsAsync(nodeId);

                await Task.WhenAll(stateTask, onlineTask, powerTask);

                var powerState = await stateTask;
                var isOnline = await onlineTask;
                var powerMetrics = await powerTask;

                var deviceState = new DeviceStateInfo
                {
                    NodeId = nodeId,
                    PowerState = powerState ?? "unknown",
                    IsOnline = isOnline,
                    LastUpdated = DateTime.UtcNow,
                    LastSeen = isOnline ? DateTime.UtcNow : _deviceStates.GetValueOrDefault(nodeId)?.LastSeen ?? DateTime.UtcNow,
                    LastError = null
                };

                // Add power metrics if available
                if (powerMetrics != null)
                {
                    deviceState.PowerConsumption = powerMetrics.PowerConsumption;
                    deviceState.Voltage = powerMetrics.Voltage;
                    deviceState.Current = powerMetrics.Current;
                    deviceState.EnergyToday = powerMetrics.EnergyToday;
                }

                // Update cache
                _deviceStates.AddOrUpdate(nodeId, deviceState, (key, oldValue) => deviceState);

                // Trigger event for real-time updates
                if (OnDeviceStateChanged != null)
                {
                    try
                    {
                        await OnDeviceStateChanged.Invoke(nodeId, deviceState);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error in OnDeviceStateChanged event handler for {NodeId}", nodeId);
                    }
                }

                _logger.LogInformation("Device state updated for {NodeId}: Power={PowerState}, Online={IsOnline}", 
                    nodeId, deviceState.PowerState, deviceState.IsOnline);

                return deviceState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing device state for {NodeId}", nodeId);
                
                var errorState = new DeviceStateInfo
                {
                    NodeId = nodeId,
                    PowerState = "unknown",
                    IsOnline = false,
                    LastError = ex.Message,
                    LastUpdated = DateTime.UtcNow
                };

                _deviceStates.AddOrUpdate(nodeId, errorState, (key, oldValue) => errorState);
                if (OnDeviceError != null)
                {
                    try
                    {
                        await OnDeviceError.Invoke(nodeId, ex.Message);
                    }
                    catch (Exception eventEx)
                    {
                        _logger.LogWarning(eventEx, "Error in OnDeviceError event handler for {NodeId}", nodeId);
                    }
                }

                return errorState;
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        /// <summary>
        /// Toggle device power state
        /// </summary>
        public async Task<bool> ToggleDeviceAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Toggling device {NodeId}", nodeId);

                // Get current state for event logging
                var currentState = await GetDeviceStateAsync(nodeId);
                var oldPowerState = currentState.PowerState;

                // Execute toggle command
                var success = await _matterService.ToggleDeviceAsync(nodeId);

                if (success)
                {
                    // Refresh state immediately after successful toggle
                    await RefreshDeviceStateAsync(nodeId);
                    
                    // Log the event
                    if (_eventService != null)
                    {
                        var newState = await GetDeviceStateAsync(nodeId, true);
                        await _eventService.LogDeviceEventAsync(
                            Guid.NewGuid(), // TODO: Get actual device ID
                            "Power Toggle",
                            oldPowerState,
                            newState.PowerState,
                            "Device power toggled via MaintainStatiModel",
                            "MaintainStatiModel"
                        );
                    }

                    _logger.LogInformation("Device {NodeId} toggled successfully: {OldState} -> {NewState}", 
                        nodeId, oldPowerState, currentState.PowerState);
                }
                else
                {
                    _logger.LogWarning("Failed to toggle device {NodeId}", nodeId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling device {NodeId}", nodeId);
                if (OnDeviceError != null)
                {
                    try
                    {
                        await OnDeviceError.Invoke(nodeId, ex.Message);
                    }
                    catch (Exception eventEx)
                    {
                        _logger.LogWarning(eventEx, "Error in OnDeviceError event handler for {NodeId}", nodeId);
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Get all cached device states
        /// </summary>
        public IEnumerable<DeviceStateInfo> GetAllDeviceStates()
        {
            return _deviceStates.Values.ToList();
        }

        /// <summary>
        /// Clear cached state for a device
        /// </summary>
        public void ClearDeviceState(string nodeId)
        {
            _deviceStates.TryRemove(nodeId, out _);
            _logger.LogInformation("Cleared cached state for device {NodeId}", nodeId);
        }

        /// <summary>
        /// Get device state without network call (cache only)
        /// </summary>
        public DeviceStateInfo? GetCachedDeviceState(string nodeId)
        {
            _deviceStates.TryGetValue(nodeId, out var state);
            return state;
        }

        /// <summary>
        /// Check if device state is cached and recent
        /// </summary>
        public bool IsDeviceStateCached(string nodeId, int maxAgeSeconds = 30)
        {
            if (_deviceStates.TryGetValue(nodeId, out var state))
            {
                var age = DateTime.UtcNow - state.LastUpdated;
                return age.TotalSeconds < maxAgeSeconds;
            }
            return false;
        }
    }
}
