using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace MSH.Infrastructure.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = null!;
    public int DeviceTypeId { get; set; }
    public int? RoomId { get; set; }
    public string? MatterDeviceId { get; set; }
    public string Status { get; set; } = "offline";
    public DateTime? LastSeen { get; set; }
    public DateTime? LastStateChange { get; set; }
    public JsonDocument? Configuration { get; set; }
    
    // Navigation properties
    public DeviceType DeviceType { get; set; } = null!;
    public Room? Room { get; set; }
    public ICollection<DeviceGroupMember> DeviceGroupMembers { get; set; } = new List<DeviceGroupMember>();
    public ICollection<DeviceGroup> DeviceGroups => DeviceGroupMembers.Select(m => m.DeviceGroup).ToList();
    public ICollection<DeviceState> States { get; set; } = new List<DeviceState>();
    public ICollection<DeviceEvent> Events { get; set; } = new List<DeviceEvent>();
} 