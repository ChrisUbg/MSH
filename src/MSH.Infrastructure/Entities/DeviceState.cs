using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceState : BaseEntity
{
    public Guid DeviceId { get; set; }
    
    [MaxLength(150)]
    public string StateType { get; set; } = null!; // e.g., 'power', 'temperature', 'humidity'
    
    public JsonDocument StateValue { get; set; } = null!;
    
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Device Device { get; set; } = null!;
} 