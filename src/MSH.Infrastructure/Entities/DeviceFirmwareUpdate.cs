using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceFirmwareUpdate : BaseEntity
{
    public Guid DeviceId { get; set; }
    
    public Device Device { get; set; } = null!;
    
    public Guid FirmwareUpdateId { get; set; }
    
    public FirmwareUpdate FirmwareUpdate { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string CurrentVersion { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string TargetVersion { get; set; } = null!;
    
    [MaxLength(20)]
    public string Status { get; set; } = "pending"; // "pending", "downloading", "downloaded", "installing", "completed", "failed", "rolled_back"
    
    public DateTime? DownloadStartedAt { get; set; }
    
    public DateTime? DownloadCompletedAt { get; set; }
    
    public DateTime? InstallationStartedAt { get; set; }
    
    public DateTime? InstallationCompletedAt { get; set; }
    
    public DateTime? TestCompletedAt { get; set; }
    
    public bool TestPassed { get; set; } = false;
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    public JsonDocument? TestResults { get; set; }
    
    public bool IsConfirmed { get; set; } = false;
    
    public DateTime? ConfirmedAt { get; set; }
    
    [MaxLength(50)]
    public string? ConfirmedBy { get; set; }
    
    public bool IsRollbackAvailable { get; set; } = false;
    
    public DateTime? RollbackCompletedAt { get; set; }
    
    [MaxLength(500)]
    public string? RollbackReason { get; set; }
    
    public JsonDocument? UpdateLog { get; set; }
}
