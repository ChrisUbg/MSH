using System;
using System.Text.Json;

namespace MSH.Infrastructure.Models;

public class mGroupState
{
    public string GroupId { get; set; } = null!;
    public JsonDocument State { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Source { get; set; }
    public string? Metadata { get; set; }

    public virtual mGroup Group { get; set; } = null!;

    public T? GetState<T>()
    {
        return JsonSerializer.Deserialize<T>(State.RootElement.GetRawText());
    }
} 