using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Web.Data;

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
        await _semaphore.WaitAsync();
        try
        {
            var userId = _currentUserService.UserId;
            int effectiveUserId = userId ?? 0; // Use 0 for anonymous/shared settings

            var settings = await _context.EnvironmentalSettings
                .FirstOrDefaultAsync(s => s.UserId == effectiveUserId && !s.IsDeleted);

            if (settings == null)
            {
                settings = new EnvironmentalSettings { UserId = effectiveUserId };
                _context.EnvironmentalSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ProcessIndoorTemperatureAsync(double temperature)
    {
        _logger.LogInformation("Processing indoor temperature: {Temperature}°C", temperature);
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();

            if (temperature >= 20.0)
            {
                await _notificationService.SendEnvironmentalAlertAsync(
                    userId: settings.UserId,
                    parameter: "Indoor Temperature",
                    currentValue: temperature,
                    threshold: 20.0
                );
                _logger.LogWarning("Indoor temperature critical: {Temperature}°C >= 20°C", temperature);
            }
            else if (temperature >= settings.TemperatureWarning)
            {
                await _notificationService.SendEnvironmentalAlertAsync(
                    userId: settings.UserId,
                    parameter: "Indoor Temperature",
                    currentValue: temperature,
                    threshold: settings.TemperatureWarning
                );
                _logger.LogWarning("Indoor temperature warning: {Temperature}°C >= {Warning}°C",
                    temperature, settings.TemperatureWarning);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ProcessOutdoorTemperatureAsync(double temperature)
    {
        _logger.LogInformation("Processing outdoor temperature: {Temperature}°C", temperature);
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();

            if (temperature >= 20.0)
            {
                await _notificationService.SendEnvironmentalAlertAsync(
                    userId: settings.UserId,
                    parameter: "Outdoor Temperature",
                    currentValue: temperature,
                    threshold: 20.0
                );
                _logger.LogWarning("Outdoor temperature critical: {Temperature}°C >= 20°C", temperature);
            }
            else if (temperature >= settings.TemperatureWarning)
            {
                await _notificationService.SendEnvironmentalAlertAsync(
                    userId: settings.UserId,
                    parameter: "Outdoor Temperature",
                    currentValue: temperature,
                    threshold: settings.TemperatureWarning
                );
                _logger.LogWarning("Outdoor temperature warning: {Temperature}°C >= {Warning}°C",
                    temperature, settings.TemperatureWarning);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ProcessHumidityAsync(double humidity)
    {
        _logger.LogInformation("Processing humidity: {Humidity}%", humidity);
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();

            if (humidity >= 45.0)
            {
                await _notificationService.SendEnvironmentalAlertAsync(
                    userId: settings.UserId,
                    parameter: "Humidity",
                    currentValue: humidity,
                    threshold: 45.0
                );
                _logger.LogWarning("Humidity critical: {Humidity}% >= 45%", humidity);
            }
            else if (humidity >= settings.HumidityWarning)
            {
                await _notificationService.SendEnvironmentalAlertAsync(
                    userId: settings.UserId,
                    parameter: "Humidity",
                    currentValue: humidity,
                    threshold: settings.HumidityWarning
                );
                _logger.LogWarning("Humidity warning: {Humidity}% >= {Warning}%",
                    humidity, settings.HumidityWarning);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ProcessAirQualityAsync(double co2, double voc)
    {
        _logger.LogInformation("Processing air quality - CO2: {CO2}ppm, VOC: {VOC}ppb", co2, voc);
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();

            if (co2 > settings.CO2Max)
            {
                await _notificationService.SendEnvironmentalAlertAsync(
                    userId: settings.UserId,
                    parameter: "CO2 Level",
                    currentValue: co2,
                    threshold: settings.CO2Max
                );
                _logger.LogWarning("CO2 level above threshold: {CO2}ppm > {Threshold}ppm",
                    co2, settings.CO2Max);
            }

            if (voc > settings.VOCMax)
            {
                await _notificationService.SendEnvironmentalAlertAsync(
                    userId: settings.UserId,
                    parameter: "VOC Level",
                    currentValue: voc,
                    threshold: settings.VOCMax
                );
                _logger.LogWarning("VOC level above threshold: {VOC}ppb > {Threshold}ppb",
                    voc, settings.VOCMax);
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
            _logger.LogInformation("Environmental thresholds updated");
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

    public async Task SetWarningLevelsAsync(double temperatureWarning, double humidityWarning)
    {
        await _semaphore.WaitAsync();
        try
        {
            var settings = await GetOrCreateSettingsAsync();
            settings.TemperatureWarning = temperatureWarning;
            settings.HumidityWarning = humidityWarning;
            settings.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Warning levels updated - Temperature: {Temp}°C, Humidity: {Humidity}%",
                temperatureWarning, humidityWarning);
        }
        finally
        {
            _semaphore.Release();
        }
    }
} 