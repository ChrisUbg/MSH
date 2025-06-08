using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Entities;
using MSH.Web.Data;

namespace MSH.Web.Services;

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _db;
    private const int DEFAULT_USER_ID = 1; // Default user ID for initial setup

    public RoomService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Room>> GetRoomsAsync()
    {
        var rooms = await _db.Rooms.Include(r => r.Devices).ToListAsync();
        if (!rooms.Any())
        {
            // Create default room if none exists
            var defaultRoom = new Room
            {
                Name = "Default-Room",
                Description = "Default room for unassigned devices",
                Floor = 1,
                CreatedById = DEFAULT_USER_ID
            };
            _db.Rooms.Add(defaultRoom);
            await _db.SaveChangesAsync();
            rooms.Add(defaultRoom);
        }
        return rooms;
    }

    public async Task<Room?> GetRoomAsync(int roomId) => await _db.Rooms.Include(r => r.Devices).FirstOrDefaultAsync(r => r.Id == roomId);
    
    public async Task<Room> AddRoomAsync(Room room)
    {
        room.CreatedById = DEFAULT_USER_ID;
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        return room;
    }
    
    public async Task<Room> UpdateRoomAsync(Room room)
    {
        room.UpdatedById = DEFAULT_USER_ID;
        _db.Rooms.Update(room);
        await _db.SaveChangesAsync();
        return room;
    }
    
    public async Task<bool> DeleteRoomAsync(int roomId)
    {
        var room = await _db.Rooms.FindAsync(roomId);
        if (room == null) return false;
        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> AssignDeviceToRoomAsync(int deviceId, int roomId)
    {
        var device = await _db.Devices.FindAsync(deviceId);
        var room = await _db.Rooms.FindAsync(roomId);
        if (device == null || room == null) return false;
        device.RoomId = roomId;
        await _db.SaveChangesAsync();
        return true;
    }
    
    private async Task<int> EnsureDefaultRoomAsync()
    {
        var defaultRoom = await _db.Rooms.FirstOrDefaultAsync(r => r.Name == "Default-Room");
        if (defaultRoom == null)
        {
            defaultRoom = new Room 
            { 
                Name = "Default-Room", 
                Description = "Default room for unassigned devices",
                CreatedById = DEFAULT_USER_ID
            };
            _db.Rooms.Add(defaultRoom);
            await _db.SaveChangesAsync();
        }
        return defaultRoom.Id;
    }
    
    public async Task<bool> RemoveDeviceFromRoomAsync(int deviceId)
    {
        var device = await _db.Devices.FindAsync(deviceId);
        if (device == null) return false;
        var defaultRoomId = await EnsureDefaultRoomAsync();
        device.RoomId = defaultRoomId;
        await _db.SaveChangesAsync();
        return true;
    }
} 