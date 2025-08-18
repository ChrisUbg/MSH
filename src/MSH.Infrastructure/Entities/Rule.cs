using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class Rule : BaseEntity
{
    [MaxLength(150)]
    public string Name { get; set; } = null!;
    
    [MaxLength(150)]
    public string? Description { get; set; }
    
    public JsonDocument Condition { get; set; } = null!; // Rule trigger conditions
    
    public JsonDocument Action { get; set; } = null!; // Actions to take when triggered
    
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public ICollection<RuleTrigger> Triggers { get; set; } = new List<RuleTrigger>();
} 