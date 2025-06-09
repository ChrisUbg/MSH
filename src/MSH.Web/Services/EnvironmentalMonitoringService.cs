using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Models;
using MSH.Infrastructure.Services;
using System.Text.Json;

namespace MSH.Web.Services;

public class EnvironmentalMonitoringService : IEnvironmentalMonitoringService
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<EnvironmentalMonitoringService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public EnvironmentalMonitoringService(
        INotificationService notificationService,
        ILogger<EnvironmentalMonitoringService> logger,
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
    }

    private async Task<EnvironmentalSettings> GetOrCreateSettingsAsync()
    {
        var userId = await _currentUserService.GetCurrentUserIdAsync();
        if (!userId.HasValue)
        {
            throw new InvalidOperationException("User is not authenticated");
        }

        var settings = await _context.EnvironmentalSettings
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);

        if (settings == null)
        {
            settings = new EnvironmentalSettings
            {
                UserId = userId.Value,
                IndoorTemperatureMin = 18.0,
                IndoorTemperatureMax = 24.0,
                OutdoorTemperatureMin = 0.0,
                OutdoorTemperatureMax = 35.0,
                HumidityMin = 30.0,
                HumidityMax = 60.0,
                CO2Max = 1000.0,
                VOCMax = 500.0,
                TemperatureWarning = 15.0,
                HumidityWarning = 40.0,
                LastUpdated = DateTime.UtcNow
            };

            _context.EnvironmentalSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task ProcessIndoorTemperatureAsync(double temperature)
    {
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();
            
            if (temperature < settings.IndoorTemperatureMin || temperature > settings.IndoorTemperatureMax)
            {
                var message = $"Indoor temperature {temperature}°C is outside the acceptable range ({settings.IndoorTemperatureMin}°C - {settings.IndoorTemperatureMax}°C)";
                await _notificationService.SendAlertToUserAsync(settings.UserId, "Temperature Alert", message, "warning");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ProcessOutdoorTemperatureAsync(double temperature)
    {
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();
            
            if (temperature < settings.OutdoorTemperatureMin || temperature > settings.OutdoorTemperatureMax)
            {
                var message = $"Outdoor temperature {temperature}°C is outside the acceptable range ({settings.OutdoorTemperatureMin}°C - {settings.OutdoorTemperatureMax}°C)";
                await _notificationService.SendAlertToUserAsync(settings.UserId, "Temperature Alert", message, "warning");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ProcessHumidityAsync(double humidity)
    {
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();
            
            if (humidity < settings.HumidityMin || humidity > settings.HumidityMax)
            {
                var message = $"Humidity {humidity}% is outside the acceptable range ({settings.HumidityMin}% - {settings.HumidityMax}%)";
                await _notificationService.SendAlertToUserAsync(settings.UserId, "Humidity Alert", message, "warning");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ProcessAirQualityAsync(double co2, double voc)
    {
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();
            
            if (co2 > settings.CO2Max)
            {
                var message = $"CO2 level {co2}ppm exceeds the maximum threshold of {settings.CO2Max}ppm";
                await _notificationService.SendAlertToUserAsync(settings.UserId, "Air Quality Alert", message, "warning");
            }

            if (voc > settings.VOCMax)
            {
                var message = $"VOC level {voc}ppb exceeds the maximum threshold of {settings.VOCMax}ppb";
                await _notificationService.SendAlertToUserAsync(settings.UserId, "Air Quality Alert", message, "warning");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetThresholdsAsync(EnvironmentalThresholds thresholds)
    {
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();
            
            settings.IndoorTemperatureMin = thresholds.IndoorTemperatureMin;
            settings.IndoorTemperatureMax = thresholds.IndoorTemperatureMax;
            settings.OutdoorTemperatureMin = thresholds.OutdoorTemperatureMin;
            settings.OutdoorTemperatureMax = thresholds.OutdoorTemperatureMax;
            settings.HumidityMin = thresholds.HumidityMin;
            settings.HumidityMax = thresholds.HumidityMax;
            settings.CO2Max = thresholds.CO2Max;
            settings.VOCMax = thresholds.VOCMax;
            settings.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<EnvironmentalThresholds> GetThresholdsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();
            return new EnvironmentalThresholds
            {
                IndoorTemperatureMin = settings.IndoorTemperatureMin,
                IndoorTemperatureMax = settings.IndoorTemperatureMax,
                OutdoorTemperatureMin = settings.OutdoorTemperatureMin,
                OutdoorTemperatureMax = settings.OutdoorTemperatureMax,
                HumidityMin = settings.HumidityMin,
                HumidityMax = settings.HumidityMax,
                CO2Max = settings.CO2Max,
                VOCMax = settings.VOCMax
            };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<DeviceEvent>> GetDeviceEventsAsync(Guid deviceId, DateTime startTime, DateTime endTime)
    {
        return await _context.DeviceEvents
            .Where(e => e.DeviceId == deviceId && e.CreatedAt >= startTime && e.CreatedAt <= endTime)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    private async Task LogDeviceEventAsync(Guid deviceId, string eventType, Dictionary<string, object> eventData)
    {
        var deviceEvent = new DeviceEvent
        {
            DeviceId = deviceId,
            EventType = eventType,
            EventData = JsonSerializer.SerializeToDocument(eventData)
        };

        _context.DeviceEvents.Add(deviceEvent);
        await _context.SaveChangesAsync();
    }
} 