using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSH.Infrastructure.Models;

namespace MSH.Infrastructure.Services;

public abstract class SimulatedDevice
{
    public string DeviceId { get; protected set; }
    public string Name { get; protected set; }
    public string Type { get; protected set; }
    public Dictionary<string, object> State { get; protected set; }
    public DateTime LastUpdated { get; protected set; }

    protected SimulatedDevice(string deviceId, string name, string type)
    {
        DeviceId = deviceId;
        Name = name;
        Type = type;
        State = new Dictionary<string, object>();
        LastUpdated = DateTime.UtcNow;
    }

    public abstract Task<bool> UpdateStateAsync(Dictionary<string, object> newState);
    public abstract Task<bool> ToggleAsync();
    public abstract Task<Dictionary<string, object>> GetStateAsync();
    public abstract Task SimulateBehaviorAsync();

    protected void UpdateLastUpdated()
    {
        LastUpdated = DateTime.UtcNow;
    }
} 