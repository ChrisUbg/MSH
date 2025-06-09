using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetRoomsAsync();
    Task<Room?> GetRoomAsync(Guid roomId);
    Task<Room> AddRoomAsync(Room room);
    Task<Room> UpdateRoomAsync(Room room);
    Task<bool> DeleteRoomAsync(Guid roomId);
    Task<bool> AssignDeviceToRoomAsync(Guid deviceId, Guid? roomId);
    Task<bool> RemoveDeviceFromRoomAsync(Guid deviceId);
} 