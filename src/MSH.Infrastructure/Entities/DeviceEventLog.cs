using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceEventLog : BaseEntity
{
    [Required]
    public Guid DeviceId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Event { get; set; } = string.Empty;
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? EventType { get; set; } // e.g., "StateChange", "Command", "Error", "Info"
    
    [MaxLength(50)]
    public string? Severity { get; set; } // e.g., "Info", "Warning", "Error", "Critical"
    
    [Column(TypeName = "jsonb")]
    public JsonDocument? EventData { get; set; } // Additional event-specific data
    
    [Column(TypeName = "jsonb")]
    public JsonDocument? OldState { get; set; } // Previous state if applicable
    
    [Column(TypeName = "jsonb")]
    public JsonDocument? NewState { get; set; } // New state if applicable
    
    [MaxLength(100)]
    public string? Source { get; set; } // e.g., "User", "System", "Matter", "Scheduled"
    
    [MaxLength(100)]
    public string? UserId { get; set; } // User who triggered the event if applicable
    
    // Navigation property
    public virtual Device Device { get; set; } = null!;
}
