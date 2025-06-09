using System.Collections.Generic;
using System.Linq;

namespace MSH.Infrastructure.Entities;

public class DeviceGroup : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<DeviceGroupMember> DeviceGroupMembers { get; set; } = new List<DeviceGroupMember>();
    public ICollection<Device> Devices => DeviceGroupMembers.Select(m => m.Device).ToList();
} 