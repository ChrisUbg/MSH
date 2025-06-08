using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Entities;
using MSH.Web.Data;
using System.Diagnostics;

namespace MSH.Web.Services;

public class DeviceService : IDeviceService
{
    private readonly ApplicationDbContext _db;
    public DeviceService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Device>> GetDevicesAsync() => await _db.Devices.Include(d => d.Room).ToListAsync();
    
    public async Task<List<Device>> GetUnassignedDevicesAsync()
    {
        var defaultRoom = await _db.Rooms.FirstOrDefaultAsync(r => r.Name == "Default-Room");
        if (defaultRoom == null) return new List<Device>();
        return await _db.Devices.Where(d => d.RoomId == defaultRoom.Id).ToListAsync();
    }
    public async Task<Device?> GetDeviceAsync(int deviceId) => await _db.Devices.Include(d => d.Room).FirstOrDefaultAsync(d => d.Id == deviceId);

    public async Task AddDeviceAsync(Device device)
    {
        _db.Devices.Add(device);
        await _db.SaveChangesAsync();
    }

    public async Task<List<DeviceType>> GetDeviceTypesAsync()
    {
        return await _db.DeviceTypes.ToListAsync();
    }
} 