using System.ComponentModel.DataAnnotations;

namespace MSH.Infrastructure.Entities;

public class DeviceActionDelay : BaseEntity
{
    public Guid DeviceId { get; set; }
    
    public Device Device { get; set; } = null!;
    
    public Guid? DeviceGroupId { get; set; }
    
    public DeviceGroup? DeviceGroup { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = null!; // "on", "off", "toggle", "brightness", "color"
    
    [Required]
    public int DelaySeconds { get; set; } = 0;
    
    [MaxLength(500)]
    public string? ActionParameters { get; set; } // JSON string for additional parameters
    
    public bool IsEnabled { get; set; } = true;
    
    public DateTime? LastExecuted { get; set; }
    
    public DateTime? NextScheduledExecution { get; set; }
    
    [MaxLength(500)]
    public string? ExecutionResult { get; set; }
    
    public bool IsRecurring { get; set; } = false;
    
    public int? RecurrenceIntervalSeconds { get; set; }
    
    [MaxLength(100)]
    public string? CronExpression { get; set; } // For complex scheduling
    
    public bool ExecuteOnStartup { get; set; } = false;
    
    public int Priority { get; set; } = 0; // Higher number = higher priority
}
