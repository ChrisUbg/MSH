using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Interfaces;

namespace MSH.Infrastructure.Services;

public class DeviceEventDelayService : IDeviceEventDelayService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceEventDelayService> _logger;

    public DeviceEventDelayService(ApplicationDbContext context, ILogger<DeviceEventDelayService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DeviceEventDelay>> GetEventDelaysForDeviceAsync(Guid deviceId)
    {
        return await _context.DeviceEventDelays
            .Where(d => d.DeviceId == deviceId && !d.IsDeleted)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<DeviceEventDelay?> GetEventDelayByIdAsync(Guid id)
    {
        return await _context.DeviceEventDelays
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async Task<DeviceEventDelay> CreateEventDelayAsync(DeviceEventDelay eventDelay)
    {
        eventDelay.Id = Guid.NewGuid();
        eventDelay.CreatedAt = DateTime.UtcNow;
        eventDelay.UpdatedAt = DateTime.UtcNow;
        eventDelay.IsDeleted = false;

        _context.DeviceEventDelays.Add(eventDelay);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created event delay {EventDelayId} for device {DeviceId}, event type {EventType}", 
            eventDelay.Id, eventDelay.DeviceId, eventDelay.EventType);

        return eventDelay;
    }

    public async Task<DeviceEventDelay> UpdateEventDelayAsync(DeviceEventDelay eventDelay)
    {
        var existing = await _context.DeviceEventDelays.FindAsync(eventDelay.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Event delay with ID {eventDelay.Id} not found");
        }

        existing.EventType = eventDelay.EventType;
        existing.DelaySeconds = eventDelay.DelaySeconds;
        existing.IsEnabled = eventDelay.IsEnabled;
        existing.Description = eventDelay.Description;
        existing.Priority = eventDelay.Priority;
        existing.Conditions = eventDelay.Conditions;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedById = eventDelay.UpdatedById;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated event delay {EventDelayId} for device {DeviceId}", 
            eventDelay.Id, eventDelay.DeviceId);

        return existing;
    }

    public async Task DeleteEventDelayAsync(Guid id)
    {
        var eventDelay = await _context.DeviceEventDelays.FindAsync(id);
        if (eventDelay != null)
        {
            eventDelay.IsDeleted = true;
            eventDelay.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted event delay {EventDelayId} for device {DeviceId}", 
                id, eventDelay.DeviceId);
        }
    }

    public async Task<IEnumerable<DeviceEventDelay>> GetActiveEventDelaysForEventTypeAsync(Guid deviceId, string eventType)
    {
        return await _context.DeviceEventDelays
            .Where(d => d.DeviceId == deviceId && 
                       d.EventType == eventType && 
                       d.IsEnabled && 
                       !d.IsDeleted)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasActiveEventDelayAsync(Guid deviceId, string eventType)
    {
        return await _context.DeviceEventDelays
            .AnyAsync(d => d.DeviceId == deviceId && 
                          d.EventType == eventType && 
                          d.IsEnabled && 
                          !d.IsDeleted);
    }
}
