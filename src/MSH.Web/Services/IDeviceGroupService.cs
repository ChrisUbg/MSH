using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Models;

namespace MSH.Web.Services
{
    public interface IDeviceGroupService
    {
        Task<List<DeviceGroup>> GetDeviceGroupsAsync();
        Task<DeviceGroup> GetDeviceGroupAsync(Guid id);
        Task<DeviceGroup> GetDeviceGroupByNameAsync(string name);
        Task<List<DeviceType>> GetDeviceTypesByGroupAsync(Guid groupId);
        Task<DeviceGroup> AddDeviceGroupAsync(DeviceGroup deviceGroup);
        Task<DeviceGroup> UpdateDeviceGroupAsync(DeviceGroup deviceGroup);
        Task<bool> DeleteDeviceGroupAsync(Guid id);
        Task<bool> AddDeviceToGroupAsync(Guid deviceId, Guid groupId);
        Task<bool> RemoveDeviceFromGroupAsync(Guid deviceId, Guid groupId);
        Task<bool> SetDevicesForGroupAsync(Guid groupId, List<Guid> deviceIds);
        Task<GroupHealthStatus> GetGroupHealthStatusAsync(Guid groupId);
        Task<bool> SetGroupStateAsync(Guid groupId, string state);
        Task<bool> SetGroupBrightnessAsync(Guid groupId, int brightness);
        Task<bool> SetGroupColorAsync(Guid groupId, string color);
        Task<Dictionary<string, object>> GetGroupStateAsync(Guid groupId);
        Task<List<DeviceGroup>> GetDeviceGroupsWithDeviceTypesAsync();
        Task<Dictionary<string, object>> GetDeviceCapabilitiesAsync(Guid deviceTypeId);
        Task<string> GetDeviceIconAsync(string deviceTypeName);
        Task<CommissioningMethod> GetPreferredCommissioningMethodAsync(Guid deviceTypeId);
        Task EnsureDefaultDeviceGroupsAsync();
    }

    public enum CommissioningMethod
    {
        BLE_WiFi,
        WiFi_Only,
        QR_Code,
        Manual_Code
    }
} 