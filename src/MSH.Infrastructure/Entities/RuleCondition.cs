using System;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class RuleCondition : BaseEntity
{
    public Guid RuleId { get; set; }
    public Rule Rule { get; set; } = null!;
    public JsonDocument Condition { get; set; } = null!;
    public int Order { get; set; }
} 