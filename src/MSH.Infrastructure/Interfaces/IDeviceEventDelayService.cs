using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Interfaces;

public interface IDeviceEventDelayService
{
    Task<IEnumerable<DeviceEventDelay>> GetEventDelaysForDeviceAsync(Guid deviceId);
    Task<DeviceEventDelay?> GetEventDelayByIdAsync(Guid id);
    Task<DeviceEventDelay> CreateEventDelayAsync(DeviceEventDelay eventDelay);
    Task<DeviceEventDelay> UpdateEventDelayAsync(DeviceEventDelay eventDelay);
    Task DeleteEventDelayAsync(Guid id);
    Task<IEnumerable<DeviceEventDelay>> GetActiveEventDelaysForEventTypeAsync(Guid deviceId, string eventType);
    Task<bool> HasActiveEventDelayAsync(Guid deviceId, string eventType);
}
