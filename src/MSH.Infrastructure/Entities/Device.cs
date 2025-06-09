using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace MSH.Infrastructure.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? MatterDeviceId { get; set; }
    public Guid DeviceTypeId { get; set; }
    public DeviceType DeviceType { get; set; } = null!;
    public Guid? RoomId { get; set; }
    public Room? Room { get; set; }
    public JsonDocument Properties { get; set; } = null!;
    public string Status { get; set; } = "unknown";
    public JsonDocument? Configuration { get; set; }
    public DateTime LastStateChange { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    
    // Navigation properties
    public ICollection<DeviceGroupMember> DeviceGroupMembers { get; set; } = new List<DeviceGroupMember>();
    public ICollection<DeviceGroup> DeviceGroups => DeviceGroupMembers.Select(m => m.DeviceGroup).ToList();
    public ICollection<DeviceState> States { get; set; } = new List<DeviceState>();
    public ICollection<DeviceEvent> Events { get; set; } = new List<DeviceEvent>();
} 