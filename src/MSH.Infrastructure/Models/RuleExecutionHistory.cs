using System;
using System.Text.Json;

namespace MSH.Infrastructure.Models;

public class mRuleExecutionHistory
{
    public int Id { get; set; }
    public string RuleId { get; set; } = null!;
    public DateTime ExecutionTime { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public JsonDocument? Result { get; set; }
    public string? TriggeredBy { get; set; }
    public string? Source { get; set; }
    public string? Metadata { get; set; }

    public virtual mRule Rule { get; set; } = null!;

    public T? GetResult<T>()
    {
        if (Result == null) return default;
        return JsonSerializer.Deserialize<T>(Result.RootElement.GetRawText());
    }

    public T? GetMetadata<T>()
    {
        if (string.IsNullOrEmpty(Metadata)) return default;
        return JsonSerializer.Deserialize<T>(Metadata);
    }
} 