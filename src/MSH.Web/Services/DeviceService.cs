using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Models;
using MSH.Web.Data;
using System.Diagnostics;

namespace MSH.Web.Services;

public class DeviceService : IDeviceService
{
    private readonly MSH.Infrastructure.Data.ApplicationDbContext _context;

    public DeviceService(MSH.Infrastructure.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Device>> GetDevicesAsync()
    {
        return await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.Room)
            .ToListAsync();
    }

    public async Task<IEnumerable<Device>> GetUnassignedDevicesAsync()
    {
        return await _context.Devices
            .Where(d => d.RoomId == null)
            .Include(d => d.DeviceType)
            .ToListAsync();
    }

    public async Task<Device?> GetDeviceAsync(string deviceId)
    {
        return await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.Room)
            .FirstOrDefaultAsync(d => d.MatterDeviceId == deviceId);
    }

    public async Task<Device?> GetDeviceByIdAsync(Guid deviceId)
    {
        return await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.Room)
            .FirstOrDefaultAsync(d => d.Id == deviceId);
    }

    public async Task<Device> AddDeviceAsync(Device device)
    {
        device.Id = Guid.NewGuid();
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<Device> UpdateDeviceAsync(Device device)
    {
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
        return device;
    }

    public async Task<bool> DeleteDeviceAsync(Guid deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return false;
        }

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<mDeviceHistory>> GetDeviceHistoryAsync(Guid deviceId)
    {
        var histories = await _context.DeviceHistory.Where(h => h.DeviceId == deviceId).ToListAsync();
        return histories.Select(h => new mDeviceHistory
        {
            Id = h.Id,
            DeviceId = h.DeviceId,
            PropertyName = h.EventType,
            OldValue = h.OldState?.RootElement.GetRawText(),
            NewValue = h.NewState?.RootElement.GetRawText(),
            Timestamp = h.CreatedAt,
            UserId = h.CreatedById.ToString(),
            Source = "DeviceService",
            Metadata = h.Description
        });
    }

    public async Task<bool> AssignDeviceToRoomAsync(Guid deviceId, Guid? roomId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return false;
        }

        if (roomId.HasValue)
        {
            var room = await _context.Rooms.FindAsync(roomId.Value);
            if (room == null)
            {
                return false;
            }
        }

        device.RoomId = roomId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveDeviceFromRoomAsync(Guid deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return false;
        }

        device.RoomId = null;
        await _context.SaveChangesAsync();
        return true;
    }
} 