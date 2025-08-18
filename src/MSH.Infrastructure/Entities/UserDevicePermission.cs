using System;
using System.ComponentModel.DataAnnotations;

namespace MSH.Infrastructure.Entities;

public class UserDevicePermission : BaseEntity
{
    [MaxLength(500)]
    public string UserId { get; set; } = null!;
    
    public Guid DeviceId { get; set; }
    
    [MaxLength(150)]
    public string PermissionLevel { get; set; } = null!; // e.g., 'read', 'write', 'admin'
    
    // Navigation properties
    public User User { get; set; } = null!;
    
    public Device Device { get; set; } = null!;
} 