using System;
using System.Text.Json;

namespace MSH.Infrastructure.Models;

public class mDeviceHistory
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string PropertyName { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? Source { get; set; }
    public string? Metadata { get; set; }

    public T? GetOldValue<T>()
    {
        if (string.IsNullOrEmpty(OldValue)) return default;
        return JsonSerializer.Deserialize<T>(OldValue);
    }

    public T? GetNewValue<T>()
    {
        if (string.IsNullOrEmpty(NewValue)) return default;
        return JsonSerializer.Deserialize<T>(NewValue);
    }

    public T? GetMetadata<T>()
    {
        if (string.IsNullOrEmpty(Metadata)) return default;
        return JsonSerializer.Deserialize<T>(Metadata);
    }
} 