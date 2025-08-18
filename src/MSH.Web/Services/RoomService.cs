using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Web.Data;
using Microsoft.Extensions.Logging;

namespace MSH.Web.Services;

public class RoomService : IRoomService
{
    private readonly MSH.Infrastructure.Data.ApplicationDbContext _context;
    private readonly ILogger<RoomService> _logger;
    private static readonly string DEFAULT_USER_ID = "00000000-0000-0000-0000-000000000001"; // Default user ID for initial setup

    public RoomService(MSH.Infrastructure.Data.ApplicationDbContext context, ILogger<RoomService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Room>> GetRoomsAsync()
    {
        try
        {
            return await _context.Rooms
                .Include(r => r.Devices)
                .ToListAsync();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database context was disposed while trying to get rooms");
            return new List<Room>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rooms from database");
            return new List<Room>();
        }
    }

    public async Task<Room?> GetRoomAsync(Guid roomId)
    {
        try
        {
            return await _context.Rooms
                .Include(r => r.Devices)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database context was disposed while trying to get room {RoomId}", roomId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room {RoomId} from database", roomId);
            return null;
        }
    }

    public async Task<Room> AddRoomAsync(Room room)
    {
        try
        {
            if (room.Id == Guid.Empty)
            {
                room.Id = Guid.NewGuid();
            }
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database context was disposed while trying to add room");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding room to database");
            throw;
        }
    }

    public async Task<Room> UpdateRoomAsync(Room room)
    {
        try
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            return room;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database context was disposed while trying to update room");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating room in database");
            throw;
        }
    }

    public async Task<bool> DeleteRoomAsync(Guid roomId)
    {
        try
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
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database context was disposed while trying to delete room {RoomId}", roomId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room {RoomId} from database", roomId);
            return false;
        }
    }

    public async Task<bool> AssignDeviceToRoomAsync(Guid deviceId, Guid? roomId)
    {
        try
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
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database context was disposed while trying to assign device {DeviceId} to room {RoomId}", deviceId, roomId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning device {DeviceId} to room {RoomId}", deviceId, roomId);
            return false;
        }
    }

    private async Task<Guid> EnsureDefaultRoomAsync()
    {
        try
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
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database context was disposed while trying to ensure default room");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring default room exists");
            throw;
        }
    }
    
    public async Task<bool> RemoveDeviceFromRoomAsync(Guid deviceId)
    {
        try
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null) return false;
            var defaultRoomId = await EnsureDefaultRoomAsync();
            device.RoomId = defaultRoomId;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Database context was disposed while trying to remove device {DeviceId} from room", deviceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing device {DeviceId} from room", deviceId);
            return false;
        }
    }
} 