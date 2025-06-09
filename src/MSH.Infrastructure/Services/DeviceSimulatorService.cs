using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public class DeviceSimulatorService : IDeviceSimulatorService, IHostedService
{
    private readonly ILogger<DeviceSimulatorService> _logger;
    private readonly Dictionary<string, SimulatedDevice> _devices;
    private Timer _simulationTimer = null!;
    private readonly ApplicationDbContext _context;
    private readonly Random _random = new Random();

    public DeviceSimulatorService(ILogger<DeviceSimulatorService> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _devices = new Dictionary<string, SimulatedDevice>();
        _context = context;
        InitializeDefaultDevices();
    }

    private void InitializeDefaultDevices()
    {
        // Add some default simulated devices
        // NOTE: DeviceTypeId should be set to a valid DeviceType Guid for simulation
        AddSimulatedDeviceAsync(new Device
        {
            MatterDeviceId = "light-1",
            Name = "Living Room Light",
            // DeviceTypeId = <set a valid Guid here>
        }).Wait();

        AddSimulatedDeviceAsync(new Device
        {
            MatterDeviceId = "temp-1",
            Name = "Living Room Temperature Sensor",
            // DeviceTypeId = <set a valid Guid here>
        }).Wait();
    }

    public async Task<IEnumerable<Device>> GetSimulatedDevicesAsync()
    {
        return await _context.Devices
            .Where(d => d.DeviceType.IsSimulated)
            .Include(d => d.DeviceType)
            .ToListAsync();
    }

    public async Task<Device?> GetSimulatedDeviceAsync(string deviceId)
    {
        return await _context.Devices
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.MatterDeviceId == deviceId && d.DeviceType.IsSimulated);
    }

    public Task<Device?> GetDeviceAsync(string deviceId)
    {
        return GetSimulatedDeviceAsync(deviceId);
    }

    public async Task<bool> UpdateDeviceStateAsync(string deviceId, Dictionary<string, object> newState)
    {
        var device = await GetSimulatedDeviceAsync(deviceId);
        if (device == null) return false;

        var state = new DeviceState
        {
            DeviceId = device.Id,
            StateType = "power",
            StateValue = JsonDocument.Parse(JsonSerializer.Serialize(newState)),
            RecordedAt = DateTime.UtcNow
        };

        _context.DeviceStates.Add(state);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleDeviceAsync(string deviceId)
    {
        var device = await GetSimulatedDeviceAsync(deviceId);
        if (device == null) return false;

        var state = new DeviceState
        {
            DeviceId = device.Id,
            StateType = "power",
            StateValue = JsonDocument.Parse(_random.Next(2) == 1 ? "true" : "false"),
            RecordedAt = DateTime.UtcNow
        };

        _context.DeviceStates.Add(state);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<string, object>?> GetDeviceStateAsync(string deviceId)
    {
        var device = await GetSimulatedDeviceAsync(deviceId);
        if (device == null) return null;

        var state = await _context.DeviceStates
            .Where(s => s.DeviceId == device.Id)
            .OrderByDescending(s => s.RecordedAt)
            .FirstOrDefaultAsync();

        if (state == null) return null;

        return JsonSerializer.Deserialize<Dictionary<string, object>>(state.StateValue.RootElement.GetRawText());
    }

    public async Task<bool> AddSimulatedDeviceAsync(Device device)
    {
        try
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding simulated device");
            return false;
        }
    }

    public async Task<bool> RemoveSimulatedDeviceAsync(string deviceId)
    {
        var device = await GetSimulatedDeviceAsync(deviceId);
        if (device == null) return false;

        try
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing simulated device");
            return false;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Device Simulator Service is starting.");

        _simulationTimer = new Timer(SimulateDevices, null, TimeSpan.Zero, 
            TimeSpan.FromSeconds(5)); // Update every 5 seconds

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Device Simulator Service is stopping.");

        _simulationTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private void SimulateDevices(object? state)
    {
        foreach (var device in _devices.Values)
        {
            try
            {
                device.SimulateBehaviorAsync().Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error simulating device {device.DeviceId}");
            }
        }
    }

    public async Task UpdateSimulatedDeviceStateAsync(string deviceId)
    {
        var device = await GetSimulatedDeviceAsync(deviceId);
        if (device == null) return;

        var state = new DeviceState
        {
            DeviceId = device.Id,
            StateType = "power",
            StateValue = JsonDocument.Parse(_random.Next(2) == 1 ? "true" : "false"),
            RecordedAt = DateTime.UtcNow
        };

        _context.DeviceStates.Add(state);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Device>> GetSimulatedDevicesByTypeAsync(string deviceType)
    {
        return await _context.Devices
            .Where(d => d.DeviceType.Name == deviceType && d.DeviceType.IsSimulated)
            .Include(d => d.DeviceType)
            .ToListAsync();
    }
} 