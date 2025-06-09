using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Models;

namespace MSH.Web.Services;

public interface IDeviceService
{
    Task<IEnumerable<Device>> GetDevicesAsync();
    Task<IEnumerable<Device>> GetUnassignedDevicesAsync();
    Task<Device?> GetDeviceAsync(string deviceId);
    Task<Device?> GetDeviceByIdAsync(Guid deviceId);
    Task<Device> AddDeviceAsync(Device device);
    Task<Device> UpdateDeviceAsync(Device device);
    Task<IEnumerable<mDeviceHistory>> GetDeviceHistoryAsync(Guid deviceId);
    Task<bool> AssignDeviceToRoomAsync(Guid deviceId, Guid? roomId);
    Task<bool> RemoveDeviceFromRoomAsync(Guid deviceId);
    Task<bool> DeleteDeviceAsync(Guid deviceId);
} 