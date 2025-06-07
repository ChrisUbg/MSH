using System.Collections.Generic;

namespace MSH.Infrastructure.Entities;

public class Room : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? Floor { get; set; }
    
    // Navigation properties
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<DeviceGroup> DeviceGroups { get; set; } = new List<DeviceGroup>();
} 