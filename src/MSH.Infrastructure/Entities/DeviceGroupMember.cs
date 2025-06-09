using System;

namespace MSH.Infrastructure.Entities;

public class DeviceGroupMember
{
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    
    public Guid DeviceGroupId { get; set; }
    public DeviceGroup DeviceGroup { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public User? UpdatedBy { get; set; }
    public string? Comment { get; set; }
} 