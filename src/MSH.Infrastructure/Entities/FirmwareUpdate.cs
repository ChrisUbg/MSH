using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class FirmwareUpdate : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string CurrentVersion { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string TargetVersion { get; set; } = null!;
    
    [MaxLength(200)]
    public string? DownloadUrl { get; set; }
    
    [MaxLength(100)]
    public string? FileName { get; set; }
    
    public long? FileSize { get; set; }
    
    [MaxLength(100)]
    public string? Checksum { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "available"; // "available", "downloading", "downloaded", "installing", "completed", "failed"
    
    public DateTime? DownloadStartedAt { get; set; }
    
    public DateTime? DownloadCompletedAt { get; set; }
    
    public DateTime? InstallationStartedAt { get; set; }
    
    public DateTime? InstallationCompletedAt { get; set; }
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    public JsonDocument? UpdateMetadata { get; set; }
    
    public bool IsCompatible { get; set; } = true;
    
    public bool RequiresConfirmation { get; set; } = false;
    
    public bool IsConfirmed { get; set; } = false;
    
    public DateTime? ConfirmedAt { get; set; }
    
    [MaxLength(50)]
    public string? ConfirmedBy { get; set; }
    
    // Navigation properties
    public ICollection<DeviceFirmwareUpdate> DeviceUpdates { get; set; } = new List<DeviceFirmwareUpdate>();
}
