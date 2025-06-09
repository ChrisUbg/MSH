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
    
    // Group control methods
    Task<bool> SetGroupStateAsync(int groupId, string state);
    Task<bool> SetGroupBrightnessAsync(int groupId, int brightness);
    Task<bool> SetGroupColorAsync(int groupId, string color);
    Task<Dictionary<string, object>> GetGroupStateAsync(int groupId);
    
    // Group health monitoring
    Task<GroupHealthStatus> GetGroupHealthStatusAsync(int groupId);
} 