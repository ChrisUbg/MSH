using System;
using System.Text.Json;

namespace MSH.Infrastructure.Models;

public class mRuleAction
{
    public string Id { get; set; } = null!;
    public string RuleId { get; set; } = null!;
    public JsonDocument Action { get; set; } = null!;
    public int Order { get; set; }
    public string? Description { get; set; }
    public string? Metadata { get; set; }

    public virtual mRule Rule { get; set; } = null!;

    public T? GetAction<T>()
    {
        return JsonSerializer.Deserialize<T>(Action.RootElement.GetRawText());
    }
} 