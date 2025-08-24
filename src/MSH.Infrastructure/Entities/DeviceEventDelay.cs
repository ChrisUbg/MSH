using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSH.Infrastructure.Entities;

public class DeviceEventDelay : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid DeviceId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty; // "turn_off", "turn_on", "brightness_change", etc.
    
    [Required]
    public int DelaySeconds { get; set; }
    
    [Required]
    public bool IsEnabled { get; set; } = true;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Priority for handling multiple delays for the same event
    public int Priority { get; set; } = 0;
    
    // Optional conditions for when this delay should apply
    [Column(TypeName = "jsonb")]
    public string? Conditions { get; set; } // JSON conditions like time of day, device state, etc.
    

    
    // Navigation properties
    [ForeignKey("DeviceId")]
    public virtual Device? Device { get; set; }
}
