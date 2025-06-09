using System;
using System.Text.Json;

namespace MSH.Infrastructure.Models;

public class mRuleCondition
{
    public string Id { get; set; } = null!;
    public string RuleId { get; set; } = null!;
    public JsonDocument Condition { get; set; } = null!;
    public int Order { get; set; }
    public string? Description { get; set; }
    public string? Metadata { get; set; }

    public virtual mRule Rule { get; set; } = null!;

    public T? GetCondition<T>()
    {
        return JsonSerializer.Deserialize<T>(Condition.RootElement.GetRawText());
    }
} 