using Microsoft.AspNetCore.SignalR;
using MSH.Web.Services;

namespace MSH.Web.Hubs
{
    public class DeviceStateHub : Hub
    {
        private readonly IDeviceStateManager _deviceStateManager;
        private readonly ILogger<DeviceStateHub> _logger;

        public DeviceStateHub(IDeviceStateManager deviceStateManager, ILogger<DeviceStateHub> logger)
        {
            _deviceStateManager = deviceStateManager;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a device group to receive updates for specific devices
        /// </summary>
        public async Task JoinDeviceGroup(string nodeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"device_{nodeId}");
            _logger.LogInformation("Client {ConnectionId} joined device group: {NodeId}", Context.ConnectionId, nodeId);
        }

        /// <summary>
        /// Leave a device group
        /// </summary>
        public async Task LeaveDeviceGroup(string nodeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"device_{nodeId}");
            _logger.LogInformation("Client {ConnectionId} left device group: {NodeId}", Context.ConnectionId, nodeId);
        }

        /// <summary>
        /// Get current device state
        /// </summary>
        public async Task<ManagedDeviceState?> GetDeviceState(string nodeId)
        {
            try
            {
                var state = await _deviceStateManager.GetDeviceStateAsync(nodeId);
                return state;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device state for {NodeId}", nodeId);
                return null;
            }
        }

        /// <summary>
        /// Refresh device state
        /// </summary>
        public async Task<bool> RefreshDeviceState(string nodeId)
        {
            try
            {
                var success = await _deviceStateManager.RefreshDeviceStateAsync(nodeId);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing device state for {NodeId}", nodeId);
                return false;
            }
        }

        /// <summary>
        /// Toggle device power
        /// </summary>
        public async Task<bool> ToggleDevice(string nodeId)
        {
            try
            {
                var success = await _deviceStateManager.ToggleDeviceAsync(nodeId);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling device {NodeId}", nodeId);
                return false;
            }
        }


    }
}
