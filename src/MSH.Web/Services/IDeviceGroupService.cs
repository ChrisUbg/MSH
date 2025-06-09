using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface IDeviceGroupService
{
    Task<List<DeviceGroup>> GetDeviceGroupsAsync();
    Task<DeviceGroup?> GetDeviceGroupAsync(Guid groupId);
    Task<DeviceGroup> AddDeviceGroupAsync(DeviceGroup group);
    Task<DeviceGroup> UpdateDeviceGroupAsync(DeviceGroup group);
    Task<bool> DeleteDeviceGroupAsync(Guid groupId);
    Task<bool> AddDeviceToGroupAsync(Guid deviceId, Guid groupId);
    Task<bool> RemoveDeviceFromGroupAsync(Guid deviceId, Guid groupId);
    Task<bool> SetDevicesForGroupAsync(Guid groupId, List<Guid> deviceIds);
    
    // Group control methods
    Task<bool> SetGroupStateAsync(Guid groupId, string state);
    Task<bool> SetGroupBrightnessAsync(Guid groupId, int brightness);
    Task<bool> SetGroupColorAsync(Guid groupId, string color);
    Task<Dictionary<string, object>> GetGroupStateAsync(Guid groupId);
    
    // Group health monitoring
    Task<GroupHealthStatus> GetGroupHealthStatusAsync(Guid groupId);
} 