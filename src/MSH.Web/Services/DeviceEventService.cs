using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MSH.Web.Services;

public class DeviceEventService : IDeviceEventService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceEventService> _logger;

    public DeviceEventService(ApplicationDbContext context, ILogger<DeviceEventService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogDeviceEventAsync(Guid deviceId, string eventType, string? oldValue = null, string? newValue = null, string? description = null, string? source = null)
    {
        try
        {
            var deviceEvent = new DeviceHistory
            {
                Id = Guid.NewGuid(),
                DeviceId = deviceId,
                EventType = eventType,
                OldState = !string.IsNullOrEmpty(oldValue) ? JsonDocument.Parse(oldValue) : null,
                NewState = !string.IsNullOrEmpty(newValue) ? JsonDocument.Parse(newValue) : null,
                Description = description ?? $"{eventType} event",
                CreatedAt = DateTime.UtcNow,
                CreatedById = "system" // TODO: Get from current user context
            };

            _context.DeviceHistory.Add(deviceEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logged device event: {EventType} for device {DeviceId}", eventType, deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log device event: {EventType} for device {DeviceId}", eventType, deviceId);
        }
    }

    public async Task<IEnumerable<mDeviceHistory>> GetDeviceEventsAsync(Guid deviceId, int limit = 50)
    {
        try
        {
            var events = await _context.DeviceHistory
                .Where(h => h.DeviceId == deviceId && !h.IsDeleted)
                .OrderByDescending(h => h.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return events.Select(h => new mDeviceHistory
            {
                Id = h.Id,
                DeviceId = h.DeviceId,
                PropertyName = h.EventType,
                OldValue = h.OldState?.RootElement.GetRawText(),
                NewValue = h.NewState?.RootElement.GetRawText(),
                Timestamp = h.CreatedAt,
                UserId = h.CreatedById,
                Source = "DeviceEventService",
                Metadata = h.Description
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get device events for device {DeviceId}", deviceId);
            return Enumerable.Empty<mDeviceHistory>();
        }
    }

    public async Task<IEnumerable<mDeviceHistory>> GetDeviceEventsAsync(Guid deviceId, DateTime startTime, DateTime endTime)
    {
        try
        {
            var events = await _context.DeviceHistory
                .Where(h => h.DeviceId == deviceId && 
                           !h.IsDeleted &&
                           h.CreatedAt >= startTime && 
                           h.CreatedAt <= endTime)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return events.Select(h => new mDeviceHistory
            {
                Id = h.Id,
                DeviceId = h.DeviceId,
                PropertyName = h.EventType,
                OldValue = h.OldState?.RootElement.GetRawText(),
                NewValue = h.NewState?.RootElement.GetRawText(),
                Timestamp = h.CreatedAt,
                UserId = h.CreatedById,
                Source = "DeviceEventService",
                Metadata = h.Description
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get device events for device {DeviceId} between {StartTime} and {EndTime}", deviceId, startTime, endTime);
            return Enumerable.Empty<mDeviceHistory>();
        }
    }
}
