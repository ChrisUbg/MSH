using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using MSH.Web.Hubs;
using MSH.Infrastructure.Interfaces;

namespace MSH.Web.Services
{
    public class DeviceStateManager : IDeviceStateManager, IDisposable
    {
        private readonly ILogger<DeviceStateManager> _logger;
        private readonly IMatterDeviceControlService _matterService;
        private readonly IHubContext<DeviceStateHub> _hubContext;
        private readonly ConcurrentDictionary<string, ManagedDeviceState> _deviceStates = new();
        private readonly SemaphoreSlim _stateLock = new(1, 1);
        private readonly Timer _validationTimer;
        private readonly TimeSpan _validationInterval = TimeSpan.FromSeconds(30);

        public DeviceStateManager(ILogger<DeviceStateManager> logger, IMatterDeviceControlService matterService, IHubContext<DeviceStateHub> hubContext)
        {
            _logger = logger;
            _matterService = matterService;
            _hubContext = hubContext;
            
            // Start periodic state validation
            _validationTimer = new Timer(ValidateAllDeviceStates, null, _validationInterval, _validationInterval);
        }

        public async Task<ManagedDeviceState> GetDeviceStateAsync(string nodeId)
        {
            if (_deviceStates.TryGetValue(nodeId, out var state))
            {
                return state;
            }

            // Initialize state if not exists
            await _stateLock.WaitAsync();
            try
            {
                if (!_deviceStates.TryGetValue(nodeId, out state))
                {
                    state = new ManagedDeviceState
                    {
                        NodeId = nodeId,
                        LastUpdated = DateTime.UtcNow,
                        Status = DeviceStatus.Unknown
                    };
                    _deviceStates[nodeId] = state;
                }
                return state;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        public async Task UpdateDeviceStateAsync(string nodeId, DeviceStateUpdate update)
        {
            await _stateLock.WaitAsync();
            try
            {
                var currentState = await GetDeviceStateAsync(nodeId);
                
                // Apply updates with validation
                if (update.PowerState.HasValue)
                {
                    currentState.PowerState = update.PowerState.Value;
                    currentState.LastPowerUpdate = DateTime.UtcNow;
                }
                
                if (update.Online.HasValue)
                {
                    currentState.IsOnline = update.Online.Value;
                    currentState.LastOnlineUpdate = DateTime.UtcNow;
                }
                
                if (update.PowerConsumption.HasValue)
                {
                    currentState.PowerConsumption = update.PowerConsumption.Value;
                    currentState.LastPowerConsumptionUpdate = DateTime.UtcNow;
                }
                
                if (update.NetworkStatus.HasValue)
                {
                    currentState.NetworkStatus = update.NetworkStatus.Value;
                    currentState.LastNetworkUpdate = DateTime.UtcNow;
                }

                currentState.LastUpdated = DateTime.UtcNow;
                
                // Determine overall status
                var previousStatus = currentState.Status;
                currentState.Status = DetermineOverallStatus(currentState);
                
                _logger.LogInformation("Device {NodeId} state updated: {PreviousStatus} -> {NewStatus}", 
                    nodeId, previousStatus, currentState.Status);
                
                // Notify UI of state change via SignalR
                try
                {
                    if (_hubContext != null)
                    {
                        var stateDto = new DeviceStateDto
                        {
                            NodeId = currentState.NodeId,
                            PowerState = currentState.PowerState.ToString(),
                            IsOnline = currentState.IsOnline,
                            NetworkStatus = currentState.NetworkStatus.ToString(),
                            PowerConsumption = currentState.PowerConsumption,
                            Status = currentState.Status.ToString(),
                            LastUpdated = currentState.LastUpdated
                        };
                        await _hubContext.Clients.All.SendAsync("DeviceStateChanged", nodeId, stateDto);
                    }
                }
                catch (Exception hubEx)
                {
                    _logger.LogWarning(hubEx, "Failed to notify UI of device state change for {NodeId}", nodeId);
                }
            }
            finally
            {
                _stateLock.Release();
            }
        }

        public async Task<bool> RefreshDeviceStateAsync(string nodeId)
        {
            try
            {
                await ValidateDeviceStateAsync(nodeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh device {NodeId} state", nodeId);
                return false;
            }
        }

        public async Task<bool> ToggleDeviceAsync(string nodeId)
        {
            await _stateLock.WaitAsync();
            try
            {
                var currentState = await GetDeviceStateAsync(nodeId);
                
                // Step 1: Validate device is reachable before attempting toggle
                if (!await ValidateDeviceReachabilityAsync(nodeId))
                {
                    _logger.LogWarning("Device {NodeId} is not reachable, cannot toggle", nodeId);
                    // Update state to reflect unreachable status
                    await UpdateDeviceStateAsync(nodeId, new DeviceStateUpdate
                    {
                        Online = false,
                        NetworkStatus = NetworkStatus.Unreachable
                    });
                    return false;
                }

                // Step 2: Perform the toggle
                var success = await _matterService.ToggleDeviceAsync(nodeId);
                if (success)
                {
                    // Step 3: Immediately validate the new state
                    await ValidateDeviceStateAsync(nodeId);
                    _logger.LogInformation("Device {NodeId} toggle completed successfully", nodeId);
                }
                else
                {
                    _logger.LogWarning("Device {NodeId} toggle command failed", nodeId);
                    // Update state to reflect failed toggle
                    await UpdateDeviceStateAsync(nodeId, new DeviceStateUpdate
                    {
                        Online = false,
                        NetworkStatus = NetworkStatus.Unreachable
                    });
                }
                
                return success;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        private async Task ValidateDeviceStateAsync(string nodeId)
        {
            try
            {
                // Get real device state from Matter
                var actualPowerState = await _matterService.GetDeviceStateAsync(nodeId);
                var isReachable = await _matterService.IsDeviceOnlineAsync(nodeId);
                
                // Update our state with ground truth
                await UpdateDeviceStateAsync(nodeId, new DeviceStateUpdate
                {
                    PowerState = actualPowerState == "on" ? PowerState.On : PowerState.Off,
                    Online = isReachable,
                    NetworkStatus = isReachable ? NetworkStatus.Reachable : NetworkStatus.Unreachable
                });
                
                _logger.LogInformation("Device {NodeId} state validated: Power={PowerState}, Online={Online}", 
                    nodeId, actualPowerState, isReachable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate device {NodeId} state", nodeId);
                
                // Mark as offline if validation fails
                await UpdateDeviceStateAsync(nodeId, new DeviceStateUpdate
                {
                    Online = false,
                    NetworkStatus = NetworkStatus.Unreachable
                });
            }
        }

        private async Task<bool> ValidateDeviceReachabilityAsync(string nodeId)
        {
            try
            {
                return await _matterService.IsDeviceOnlineAsync(nodeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate device {NodeId} reachability", nodeId);
                return false;
            }
        }

        private async void ValidateAllDeviceStates(object? state)
        {
            try
            {
                var deviceIds = _deviceStates.Keys.ToList();
                foreach (var nodeId in deviceIds)
                {
                    try
                    {
                        await ValidateDeviceStateAsync(nodeId);
                    }
                    catch (Exception deviceEx)
                    {
                        _logger.LogError(deviceEx, "Error validating device {NodeId} state", nodeId);
                        // Continue with other devices
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during periodic device state validation");
            }
        }

        private DeviceStatus DetermineOverallStatus(ManagedDeviceState state)
        {
            // Priority order: Offline > Online > Error
            if (!state.IsOnline || state.NetworkStatus == NetworkStatus.Unreachable)
            {
                return DeviceStatus.Offline;
            }
            
            // If device is online and reachable, it's online regardless of power state
            return DeviceStatus.Online;
        }

        public void Dispose()
        {
            _validationTimer?.Dispose();
            _stateLock?.Dispose();
        }
    }

    public interface IDeviceStateManager
    {
        Task<ManagedDeviceState> GetDeviceStateAsync(string nodeId);
        Task UpdateDeviceStateAsync(string nodeId, DeviceStateUpdate update);
        Task<bool> RefreshDeviceStateAsync(string nodeId);
        Task<bool> ToggleDeviceAsync(string nodeId);
    }

    public class ManagedDeviceState
    {
        public string NodeId { get; set; } = string.Empty;
        public PowerState PowerState { get; set; } = PowerState.Unknown;
        public bool IsOnline { get; set; } = false;
        public NetworkStatus NetworkStatus { get; set; } = NetworkStatus.Unknown;
        public decimal PowerConsumption { get; set; } = 0;
        public DeviceStatus Status { get; set; } = DeviceStatus.Unknown;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime LastPowerUpdate { get; set; } = DateTime.UtcNow;
        public DateTime LastOnlineUpdate { get; set; } = DateTime.UtcNow;
        public DateTime LastPowerConsumptionUpdate { get; set; } = DateTime.UtcNow;
        public DateTime LastNetworkUpdate { get; set; } = DateTime.UtcNow;
    }

    public class DeviceStateUpdate
    {
        public PowerState? PowerState { get; set; }
        public bool? Online { get; set; }
        public NetworkStatus? NetworkStatus { get; set; }
        public decimal? PowerConsumption { get; set; }
    }

    public enum PowerState
    {
        Unknown,
        On,
        Off
    }

    public enum NetworkStatus
    {
        Unknown,
        Reachable,
        Unreachable
    }

    public enum DeviceStatus
    {
        Unknown,
        Online,
        Offline,
        Error
    }

    // DTO for SignalR communication
    public class DeviceStateDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string PowerState { get; set; } = string.Empty;
        public bool IsOnline { get; set; } = false;
        public string NetworkStatus { get; set; } = string.Empty;
        public decimal PowerConsumption { get; set; } = 0;
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
