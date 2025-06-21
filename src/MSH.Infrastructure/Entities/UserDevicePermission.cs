using System;

namespace MSH.Infrastructure.Entities;

public class UserDevicePermission : BaseEntity
{
    public string UserId { get; set; } = null!;
    public Guid DeviceId { get; set; }
    public string PermissionLevel { get; set; } = null!; // e.g., 'read', 'write', 'admin'
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Device Device { get; set; } = null!;
} 