using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class Cluster : BaseEntity
{
    [Required]
    public int ClusterId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Category { get; set; } // e.g., "Basic", "Measurement", "Control"
    
    public bool IsRequired { get; set; } = false;
    
    public bool IsOptional { get; set; } = true;
    
    [MaxLength(20)]
    public string? MatterVersion { get; set; } // e.g., "1.0", "1.3"
    
    public JsonDocument? Attributes { get; set; }
    
    public JsonDocument? Commands { get; set; }
    
    public JsonDocument? Events { get; set; }
}
