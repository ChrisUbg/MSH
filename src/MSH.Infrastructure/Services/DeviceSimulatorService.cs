using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Models;

namespace MSH.Infrastructure.Services;

public class DeviceSimulatorService : IDeviceSimulatorService, IHostedService
{
    private readonly ILogger<DeviceSimulatorService> _logger;
    private readonly Dictionary<string, SimulatedDevice> _devices;
    private Timer _simulationTimer = null!;

    public DeviceSimulatorService(ILogger<DeviceSimulatorService> logger)
    {
        _logger = logger;
        _devices = new Dictionary<string, SimulatedDevice>();
        InitializeDefaultDevices();
    }

    private void InitializeDefaultDevices()
    {
        // Add some default simulated devices
        AddSimulatedDeviceAsync(new Device
        {
            DeviceId = "light-1",
            Name = "Living Room Light",
            Type = "Light"
        }).Wait();

        AddSimulatedDeviceAsync(new Device
        {
            DeviceId = "temp-1",
            Name = "Living Room Temperature Sensor",
            Type = "TemperatureSensor"
        }).Wait();
    }

    public async Task<IEnumerable<Device>> GetSimulatedDevicesAsync()
    {
        return await Task.FromResult(_devices.Values.Select(d => new Device
        {
            DeviceId = d.DeviceId,
            Name = d.Name,
            Type = d.Type
        }));
    }

    public async Task<Device?> GetSimulatedDeviceAsync(string deviceId)
    {
        if (_devices.TryGetValue(deviceId, out var device))
        {
            return await Task.FromResult(new Device
            {
                DeviceId = device.DeviceId,
                Name = device.Name,
                Type = device.Type
            });
        }
        return null;
    }

    // public Task<IEnumerable<Device>> GetDevicesAsync()
    // {
    //    // throw new NotImplementedException();
    //    return GetSimulatedDevicesAsync();
    // }

    public Task<Device?> GetDeviceAsync(string deviceId)
    {
        return GetSimulatedDeviceAsync(deviceId);
    }

    public async Task<bool> UpdateDeviceStateAsync(string deviceId, Dictionary<string, object> newState)
    {
        if (_devices.TryGetValue(deviceId, out var device))
        {
            return await device.UpdateStateAsync(newState);
        }
        return false;
    }

    public async Task<bool> ToggleDeviceAsync(string deviceId)
    {
        if (_devices.TryGetValue(deviceId, out var device))
        {
            return await device.ToggleAsync();
        }
        return false;
    }

    public async Task<Dictionary<string, object>?> GetDeviceStateAsync(string deviceId)
    {
        if (_devices.TryGetValue(deviceId, out var device))
        {
            return await device.GetStateAsync();
        }
        return null;
    }

    public async Task<bool> AddSimulatedDeviceAsync(Device device)
    {
        if (_devices.ContainsKey(device.DeviceId))
        {
            return await Task.FromResult(false);
        }

        SimulatedDevice simulatedDevice = device.Type switch
        {
            "Light" => new SimulatedLight(device.DeviceId, device.Name),
            "TemperatureSensor" => new SimulatedTemperatureSensor(device.DeviceId, device.Name),
            _ => throw new ArgumentException($"Unsupported device type: {device.Type}")
        };

        _devices.Add(device.DeviceId, simulatedDevice);
        return await Task.FromResult(true);
    }

    public async Task<bool> RemoveSimulatedDeviceAsync(string deviceId)
    {
        return await Task.FromResult(_devices.Remove(deviceId));
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
} 