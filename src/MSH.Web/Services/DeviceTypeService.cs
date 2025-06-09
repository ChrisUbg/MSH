using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public class DeviceTypeService : IDeviceTypeService
{
    private readonly ApplicationDbContext _context;

    public DeviceTypeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DeviceType>> GetDeviceTypesAsync()
    {
        return await _context.DeviceTypes
            .Where(dt => !dt.IsDeleted)
            .ToListAsync();
    }

    public async Task<DeviceType?> GetDeviceTypeAsync(Guid id)
    {
        return await _context.DeviceTypes
            .FirstOrDefaultAsync(dt => dt.Id == id && !dt.IsDeleted);
    }

    public async Task<DeviceType> AddDeviceTypeAsync(DeviceType deviceType)
    {
        _context.DeviceTypes.Add(deviceType);
        await _context.SaveChangesAsync();
        return deviceType;
    }

    public async Task<DeviceType> UpdateDeviceTypeAsync(DeviceType deviceType)
    {
        _context.DeviceTypes.Update(deviceType);
        await _context.SaveChangesAsync();
        return deviceType;
    }

    public async Task DeleteDeviceTypeAsync(Guid id)
    {
        var deviceType = await _context.DeviceTypes.FindAsync(id);
        if (deviceType != null)
        {
            deviceType.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
} 