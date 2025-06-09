using System;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceHistory : BaseEntity
{
    public Guid DeviceId { get; set; }
    public string EventType { get; set; } = null!;
    public JsonDocument? OldState { get; set; }
    public JsonDocument? NewState { get; set; }
    public string? Description { get; set; }
} 