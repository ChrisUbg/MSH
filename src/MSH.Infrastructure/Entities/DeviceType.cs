using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceType : BaseEntity
{
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    
    [MaxLength(150)]
    public string? Description { get; set; }
    
    public JsonDocument Capabilities { get; set; } = null!;
    
    public bool IsSimulated { get; set; }
    
    [MaxLength(150)]
    public string Icon { get; set; } = "oi-device-hdd";
    
    [MaxLength(150)]
    public string PreferredCommissioningMethod { get; set; } = "BLE_WiFi";
    
    // Foreign key for DeviceGroup
    public Guid? DeviceGroupId { get; set; }
    
    // Navigation properties
    public DeviceGroup? DeviceGroup { get; set; }
    public ICollection<Device> Devices { get; set; } = new List<Device>();
} 