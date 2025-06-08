using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface IRoomService
{
    Task<List<Room>> GetRoomsAsync();
    Task<Room?> GetRoomAsync(int roomId);
    Task<Room> AddRoomAsync(Room room);
    Task<Room> UpdateRoomAsync(Room room);
    Task<bool> DeleteRoomAsync(int roomId);
    Task<bool> AssignDeviceToRoomAsync(int deviceId, int roomId);
    Task<bool> RemoveDeviceFromRoomAsync(int deviceId);
} 