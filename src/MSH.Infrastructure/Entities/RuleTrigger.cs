using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class RuleTrigger : BaseEntity
{
    public int RuleId { get; set; }
    public string TriggerType { get; set; } = null!; // e.g., 'device_state', 'time', 'event'
    public JsonDocument TriggerConfig { get; set; } = null!;
    
    // Navigation property
    public Rule Rule { get; set; } = null!;
} 