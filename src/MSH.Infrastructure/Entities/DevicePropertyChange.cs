using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DevicePropertyChange : BaseEntity
{
    public Guid DeviceId { get; set; }
    
    public Device Device { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string PropertyName { get; set; } = null!;
    
    public JsonDocument? OldValue { get; set; }
    
    public JsonDocument? NewValue { get; set; }
    
    [MaxLength(50)]
    public string ChangeType { get; set; } = "update"; // "add", "update", "remove"
    
    [MaxLength(200)]
    public string? Reason { get; set; } // e.g., "firmware_update", "manual_change", "discovery"
    
    public DateTime ChangeTimestamp { get; set; } = DateTime.UtcNow;
    
    public bool IsConfirmed { get; set; } = false;
    
    public DateTime? ConfirmedAt { get; set; }
    
    [MaxLength(50)]
    public string? ConfirmedBy { get; set; }
}
