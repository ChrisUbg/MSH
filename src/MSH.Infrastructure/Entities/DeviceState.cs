using System;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceState : BaseEntity
{
    public int DeviceId { get; set; }
    public string StateType { get; set; } = null!; // e.g., 'power', 'temperature', 'humidity'
    public JsonDocument StateValue { get; set; } = null!;
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Device Device { get; set; } = null!;
} 