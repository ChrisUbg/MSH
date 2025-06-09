using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface IDeviceTypeService
{
    Task<IEnumerable<DeviceType>> GetDeviceTypesAsync();
    Task<DeviceType?> GetDeviceTypeAsync(Guid id);
    Task<DeviceType> AddDeviceTypeAsync(DeviceType deviceType);
    Task<DeviceType> UpdateDeviceTypeAsync(DeviceType deviceType);
    Task DeleteDeviceTypeAsync(Guid id);
} 