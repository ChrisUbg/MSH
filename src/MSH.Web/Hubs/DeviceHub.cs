using Microsoft.AspNetCore.SignalR;

namespace MSH.Web.Hubs
{
    public class DeviceHub : Hub
    {
        public async Task SendDeviceUpdate(int deviceId)
        {
            await Clients.All.SendAsync("ReceiveDeviceUpdate", deviceId);
        }
    }
} 