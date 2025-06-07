using System.Collections.Generic;

namespace MSH.Infrastructure.Entities;

public class DeviceGroup : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int RoomId { get; set; }
    
    // Navigation properties
    public Room Room { get; set; } = null!;
    public ICollection<DeviceGroupMember> DeviceGroupMembers { get; set; } = new List<DeviceGroupMember>();
} 