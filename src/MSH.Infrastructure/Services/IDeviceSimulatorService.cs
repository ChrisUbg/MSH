using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Models;

namespace MSH.Infrastructure.Services;

public interface IDeviceSimulatorService
{
    Task<IEnumerable<Device>> GetSimulatedDevicesAsync();
    Task<Device?> GetSimulatedDeviceAsync(string deviceId);
    Task<bool> UpdateDeviceStateAsync(string deviceId, Dictionary<string, object> newState);
    Task<bool> ToggleDeviceAsync(string deviceId);
    Task<Dictionary<string, object>?> GetDeviceStateAsync(string deviceId);
    Task<bool> AddSimulatedDeviceAsync(Device device);
    Task<bool> RemoveSimulatedDeviceAsync(string deviceId);
} 