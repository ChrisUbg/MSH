using System.ComponentModel.DataAnnotations;

namespace MSH.Infrastructure.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = null!;
    
    [MaxLength(500)]
    public string Message { get; set; } = null!;
    public NotificationType Type { get; set; }
    
    public AlertSeverity? Severity { get; set; }
    
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success,
    Alert,
    DeviceStatus
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
} 