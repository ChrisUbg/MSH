using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceEvent : BaseEntity
{
    public Guid DeviceId { get; set; }
    public string EventType { get; set; } = null!;
    public JsonDocument? EventData { get; set; }
    
    // Navigation property
    public Device Device { get; set; } = null!;
} 