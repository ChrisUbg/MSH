using System.ComponentModel.DataAnnotations;

namespace MSH.Infrastructure.Entities;

public class Room : BaseEntity
{
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    
    [MaxLength(150)]
    public string? Description { get; set; }
    
    public int? Floor { get; set; }
    
    // Navigation properties
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<DeviceGroup> DeviceGroups { get; set; } = new List<DeviceGroup>();
} 