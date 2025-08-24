using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Interfaces;

namespace MSH.Web.Services
{
    public class UnifiedDeviceControlService : IUnifiedDeviceControlService
    {
        private readonly ILogger<UnifiedDeviceControlService> _logger;
        private readonly IDeviceStateManager _stateManager;
        private readonly IMatterDeviceControlService _matterService;

        public UnifiedDeviceControlService(
            ILogger<UnifiedDeviceControlService> logger,
            IDeviceStateManager stateManager,
            IMatterDeviceControlService matterService)
        {
            _logger = logger;
            _stateManager = stateManager;
            _matterService = matterService;
        }

        public async Task<DeviceControlResult> GetDeviceStatusAsync(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                return new DeviceControlResult
                {
                    Success = false,
                    ErrorMessage = "Node ID cannot be null or empty"
                };
            }

            try
            {
                var state = await _stateManager.GetDeviceStateAsync(nodeId);
                
                return new DeviceControlResult
                {
                    Success = true,
                    NodeId = nodeId,
                    PowerState = state.PowerState.ToString().ToUpper(),
                    IsOnline = state.IsOnline,
                    NetworkStatus = state.NetworkStatus.ToString().ToUpper(),
                    PowerConsumption = state.PowerConsumption,
                    OverallStatus = state.Status.ToString().ToUpper(),
                    LastUpdated = state.LastUpdated,
                    CanToggle = state.Status == DeviceStatus.Online
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get device status for {NodeId}", nodeId);
                return new DeviceControlResult
                {
                    Success = false,
                    NodeId = nodeId,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DeviceControlResult> ToggleDeviceAsync(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                return new DeviceControlResult
                {
                    Success = false,
                    ErrorMessage = "Node ID cannot be null or empty"
                };
            }

            try
            {
                _logger.LogInformation("Attempting to toggle device {NodeId}", nodeId);
                
                // Step 1: Check device reachability first (as per our process flow)
                var isReachable = await _matterService.IsDeviceOnlineAsync(nodeId);
                if (!isReachable)
                {
                    _logger.LogWarning("Device {NodeId} is not reachable, cannot toggle", nodeId);
                    return new DeviceControlResult
                    {
                        Success = false,
                        NodeId = nodeId,
                        ErrorMessage = "Device is not reachable - check network connection",
                        IsOnline = false,
                        NetworkStatus = "UNREACHABLE",
                        OverallStatus = "OFFLINE",
                        CanToggle = false
                    };
                }
                
                // Step 2: Use the state manager to handle the toggle with proper validation
                var success = await _stateManager.ToggleDeviceAsync(nodeId);
                
                if (success)
                {
                    // Step 3: Get the updated state
                    var updatedState = await _stateManager.GetDeviceStateAsync(nodeId);
                    
                    _logger.LogInformation("Device {NodeId} toggled successfully. New state: {PowerState}", 
                        nodeId, updatedState.PowerState);
                    
                    return new DeviceControlResult
                    {
                        Success = true,
                        NodeId = nodeId,
                        PowerState = updatedState.PowerState.ToString().ToUpper(),
                        IsOnline = updatedState.IsOnline,
                        NetworkStatus = updatedState.NetworkStatus.ToString().ToUpper(),
                        PowerConsumption = updatedState.PowerConsumption,
                        OverallStatus = updatedState.Status.ToString().ToUpper(),
                        LastUpdated = updatedState.LastUpdated,
                        CanToggle = updatedState.Status == DeviceStatus.Online
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to toggle device {NodeId}", nodeId);
                    return new DeviceControlResult
                    {
                        Success = false,
                        NodeId = nodeId,
                        ErrorMessage = "Device toggle failed - command execution failed",
                        IsOnline = false,
                        NetworkStatus = "UNREACHABLE",
                        OverallStatus = "OFFLINE",
                        CanToggle = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while toggling device {NodeId}", nodeId);
                return new DeviceControlResult
                {
                    Success = false,
                    NodeId = nodeId,
                    ErrorMessage = ex.Message,
                    IsOnline = false,
                    NetworkStatus = "UNKNOWN",
                    OverallStatus = "ERROR",
                    CanToggle = false
                };
            }
        }

        public async Task<DeviceControlResult> RefreshDeviceStateAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Refreshing device state for {NodeId}", nodeId);
                
                // Force a state validation
                var state = await _stateManager.GetDeviceStateAsync(nodeId);
                
                // Get real device state from Matter
                var actualPowerState = await _matterService.GetDeviceStateAsync(nodeId);
                var isReachable = await _matterService.IsDeviceOnlineAsync(nodeId);
                
                // Update state manager with ground truth
                await _stateManager.UpdateDeviceStateAsync(nodeId, new DeviceStateUpdate
                {
                    PowerState = actualPowerState?.ToLower() == "on" ? PowerState.On : PowerState.Off,
                    Online = isReachable,
                    NetworkStatus = isReachable ? NetworkStatus.Reachable : NetworkStatus.Unreachable
                });
                
                // Get updated state
                var updatedState = await _stateManager.GetDeviceStateAsync(nodeId);
                
                return new DeviceControlResult
                {
                    Success = true,
                    NodeId = nodeId,
                    PowerState = updatedState.PowerState.ToString().ToUpper(),
                    IsOnline = updatedState.IsOnline,
                    NetworkStatus = updatedState.NetworkStatus.ToString().ToUpper(),
                    PowerConsumption = updatedState.PowerConsumption,
                    OverallStatus = updatedState.Status.ToString().ToUpper(),
                    LastUpdated = updatedState.LastUpdated,
                    CanToggle = updatedState.Status == DeviceStatus.Online
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh device state for {NodeId}", nodeId);
                return new DeviceControlResult
                {
                    Success = false,
                    NodeId = nodeId,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    public interface IUnifiedDeviceControlService
    {
        Task<DeviceControlResult> GetDeviceStatusAsync(string nodeId);
        Task<DeviceControlResult> ToggleDeviceAsync(string nodeId);
        Task<DeviceControlResult> RefreshDeviceStateAsync(string nodeId);
    }

    public class DeviceControlResult
    {
        public bool Success { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string PowerState { get; set; } = "UNKNOWN";
        public bool IsOnline { get; set; } = false;
        public string NetworkStatus { get; set; } = "UNKNOWN";
        public decimal PowerConsumption { get; set; } = 0;
        public string OverallStatus { get; set; } = "UNKNOWN";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool CanToggle { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }
}
