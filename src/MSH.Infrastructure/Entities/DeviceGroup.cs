using System.Text.Json;

namespace MSH.Infrastructure.Entities;
using System.ComponentModel.DataAnnotations;
public class DeviceGroup : BaseEntity
{
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    
    [MaxLength(150)]
    public string? Description { get; set; }
    
    [MaxLength(150)]
    public string Icon { get; set; } = "oi-device-hdd";
    
    [MaxLength(150)]
    public string PreferredCommissioningMethod { get; set; } = "BLE_WiFi";
    
    public JsonDocument? DefaultCapabilities { get; set; }
    
    public int SortOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<DeviceType> DeviceTypes { get; set; } = new List<DeviceType>();
    // public ICollection<DeviceGroupMember> DeviceGroupMembers { get; set; } = new List<DeviceGroupMember>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
} 