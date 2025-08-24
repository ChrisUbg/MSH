using MSH.Infrastructure.Models;

namespace MSH.Web.Services;

public interface IDeviceEventService
{
    Task LogDeviceEventAsync(Guid deviceId, string eventType, string? oldValue = null, string? newValue = null, string? description = null, string? source = null);
    Task<IEnumerable<mDeviceHistory>> GetDeviceEventsAsync(Guid deviceId, int limit = 50);
    Task<IEnumerable<mDeviceHistory>> GetDeviceEventsAsync(Guid deviceId, DateTime startTime, DateTime endTime);
}
