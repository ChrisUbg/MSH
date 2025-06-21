using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Web.Data;

namespace MSH.Web.Services;

public class RoomService : IRoomService
{
    private readonly MSH.Infrastructure.Data.ApplicationDbContext _context;
    private static readonly string DEFAULT_USER_ID = "00000000-0000-0000-0000-000000000001"; // Default user ID for initial setup

    public RoomService(MSH.Infrastructure.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Room>> GetRoomsAsync()
    {
        return await _context.Rooms
            .Include(r => r.Devices)
            .ToListAsync();
    }

    public async Task<Room?> GetRoomAsync(Guid roomId)
    {
        return await _context.Rooms
            .Include(r => r.Devices)
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }

    public async Task<Room> AddRoomAsync(Room room)
    {
        if (room.Id == Guid.Empty)
        {
            room.Id = Guid.NewGuid();
        }
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room> UpdateRoomAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<bool> DeleteRoomAsync(Guid roomId)
    {
        var room = await _context.Rooms
            .Include(r => r.Devices)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
        {
            return false;
        }

        // Move devices to unassigned state
        foreach (var device in room.Devices)
        {
            device.RoomId = null;
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignDeviceToRoomAsync(Guid deviceId, Guid? roomId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return false;
        }

        device.RoomId = roomId;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<Guid> EnsureDefaultRoomAsync()
    {
        var defaultRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Name == "Default-Room");
        if (defaultRoom == null)
        {
            defaultRoom = new Room 
            { 
                Name = "Default-Room", 
                Description = "Default room for unassigned devices",
                CreatedById = DEFAULT_USER_ID
            };
            _context.Rooms.Add(defaultRoom);
            await _context.SaveChangesAsync();
        }
        return defaultRoom.Id;
    }
    
    public async Task<bool> RemoveDeviceFromRoomAsync(Guid deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null) return false;
        var defaultRoomId = await EnsureDefaultRoomAsync();
        device.RoomId = defaultRoomId;
        await _context.SaveChangesAsync();
        return true;
    }
} 