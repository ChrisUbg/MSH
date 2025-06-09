using System;
using System.Text.Json;

namespace MSH.Infrastructure.Models;

public class mGroupStateHistory
{
    public int Id { get; set; }
    public string GroupId { get; set; } = null!;
    public JsonDocument? OldState { get; set; }
    public JsonDocument? NewState { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? Source { get; set; }
    public string? Metadata { get; set; }

    public virtual mGroup Group { get; set; } = null!;

    public T? GetOldState<T>()
    {
        if (OldState == null) return default;
        return JsonSerializer.Deserialize<T>(OldState.RootElement.GetRawText());
    }

    public T? GetNewState<T>()
    {
        if (NewState == null) return default;
        return JsonSerializer.Deserialize<T>(NewState.RootElement.GetRawText());
    }

    public T? GetMetadata<T>()
    {
        if (string.IsNullOrEmpty(Metadata)) return default;
        return JsonSerializer.Deserialize<T>(Metadata);
    }
} 