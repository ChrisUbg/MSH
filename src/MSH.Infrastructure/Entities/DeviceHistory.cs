using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceHistory : BaseEntity
{
    public Guid DeviceId { get; set; }
    
    [MaxLength(150)]
    public string EventType { get; set; } = null!;
    
    public JsonDocument? OldState { get; set; }
    
    public JsonDocument? NewState { get; set; }
    
    [MaxLength(150)]
    public string? Description { get; set; }
} 