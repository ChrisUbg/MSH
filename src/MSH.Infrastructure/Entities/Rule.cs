using System.Collections.Generic;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class Rule : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public JsonDocument Condition { get; set; } = null!; // Rule trigger conditions
    public JsonDocument Action { get; set; } = null!; // Actions to take when triggered
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public ICollection<RuleTrigger> Triggers { get; set; } = new List<RuleTrigger>();
} 