using System;

namespace MSH.Infrastructure.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success
} 