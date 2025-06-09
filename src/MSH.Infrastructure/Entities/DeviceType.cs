using System.Collections.Generic;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class DeviceType : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public JsonDocument Capabilities { get; set; } = null!;
    public bool IsSimulated { get; set; }
    
    // Navigation properties
    public ICollection<Device> Devices { get; set; } = new List<Device>();
} 