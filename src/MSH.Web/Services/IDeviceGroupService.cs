using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface IDeviceGroupService
{
    Task<List<DeviceGroup>> GetDeviceGroupsAsync();
    Task<DeviceGroup?> GetDeviceGroupAsync(int groupId);
    Task<DeviceGroup> AddDeviceGroupAsync(DeviceGroup group);
    Task<DeviceGroup> UpdateDeviceGroupAsync(DeviceGroup group);
    Task<bool> DeleteDeviceGroupAsync(int groupId);
    Task<bool> AddDeviceToGroupAsync(int deviceId, int groupId);
    Task<bool> RemoveDeviceFromGroupAsync(int deviceId, int groupId);
    Task<bool> SetDevicesForGroupAsync(int groupId, List<int> deviceIds);
} 