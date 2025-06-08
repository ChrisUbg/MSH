using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface IDeviceService
{
    Task<IEnumerable<Device>> GetDevicesAsync();
    Task<List<Device>> GetUnassignedDevicesAsync();
    Task<Device?> GetDeviceAsync(int deviceId);
    Task AddDeviceAsync(Device device);
    Task<List<DeviceType>> GetDeviceTypesAsync();
} 