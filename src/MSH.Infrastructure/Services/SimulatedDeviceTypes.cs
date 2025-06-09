using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Models;

namespace MSH.Infrastructure.Services;

public class SimulatedLight : SimulatedDevice
{
    private bool _isOn;
    private int _brightness;
    private string _color;

    public SimulatedLight(string deviceId, string name) 
        : base(deviceId, name, "Light")
    {
        _isOn = false;
        _brightness = 100;
        _color = "#FFFFFF";
        UpdateState();
    }

    private void UpdateState()
    {
        State["isOn"] = _isOn;
        State["brightness"] = _brightness;
        State["color"] = _color;
        UpdateLastUpdated();
    }

    public override async Task<bool> UpdateStateAsync(Dictionary<string, object> newState)
    {
        if (newState.ContainsKey("isOn"))
            _isOn = (bool)newState["isOn"];
        if (newState.ContainsKey("brightness"))
            _brightness = (int)newState["brightness"];
        if (newState.ContainsKey("color"))
            _color = (string)newState["color"];

        UpdateState();
        return await Task.FromResult(true);
    }

    public override async Task<bool> ToggleAsync()
    {
        _isOn = !_isOn;
        UpdateState();
        return await Task.FromResult(true);
    }

    public override async Task<Dictionary<string, object>> GetStateAsync()
    {
        return await Task.FromResult(State);
    }

    public override async Task SimulateBehaviorAsync()
    {
        // Simulate random brightness fluctuations when on
        if (_isOn)
        {
            var random = new Random();
            _brightness = Math.Max(0, Math.Min(100, _brightness + random.Next(-5, 6)));
            UpdateState();
        }
        await Task.CompletedTask;
    }
}

public class SimulatedTemperatureSensor : SimulatedDevice
{
    private double _temperature;
    private double _humidity;

    public SimulatedTemperatureSensor(string deviceId, string name) 
        : base(deviceId, name, "TemperatureSensor")
    {
        _temperature = 20.0;
        _humidity = 50.0;
        UpdateState();
    }

    private void UpdateState()
    {
        State["temperature"] = _temperature;
        State["humidity"] = _humidity;
        UpdateLastUpdated();
    }

    public override async Task<bool> UpdateStateAsync(Dictionary<string, object> newState)
    {
        if (newState.ContainsKey("temperature"))
            _temperature = (double)newState["temperature"];
        if (newState.ContainsKey("humidity"))
            _humidity = (double)newState["humidity"];

        UpdateState();
        return await Task.FromResult(true);
    }

    public override async Task<bool> ToggleAsync()
    {
        // Temperature sensors don't toggle
        return await Task.FromResult(false);
    }

    public override async Task<Dictionary<string, object>> GetStateAsync()
    {
        return await Task.FromResult(State);
    }

    public override async Task SimulateBehaviorAsync()
    {
        // Simulate realistic temperature and humidity changes
        var random = new Random();
        _temperature += random.NextDouble() * 0.2 - 0.1;
        _humidity += random.NextDouble() * 0.5 - 0.25;
        
        // Keep values within realistic ranges
        _temperature = Math.Max(15.0, Math.Min(30.0, _temperature));
        _humidity = Math.Max(30.0, Math.Min(70.0, _humidity));
        
        UpdateState();
        await Task.CompletedTask;
    }
} 