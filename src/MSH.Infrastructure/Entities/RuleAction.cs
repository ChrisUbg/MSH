using System;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class RuleAction : BaseEntity
{
    public Guid RuleId { get; set; }
    public Rule Rule { get; set; } = null!;
    public JsonDocument Action { get; set; } = null!;
    public int Order { get; set; }
} 