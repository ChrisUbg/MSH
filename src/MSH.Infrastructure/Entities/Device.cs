using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class Device : BaseEntity
{
    
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    
    [MaxLength(150)]
    public string? Description { get; set; }
    
    [MaxLength(150)]
    public string? MatterDeviceId { get; set; }
    
    public Guid DeviceTypeId { get; set; }
    
    public DeviceType DeviceType { get; set; } = null!;
    
    public Guid? RoomId { get; set; }
    
    public Room? Room { get; set; }
    
    public JsonDocument Properties { get; set; } = null!;
    
    [MaxLength(50)]
    public string Status { get; set; } = "unknown";
    
    public JsonDocument? Configuration { get; set; }
    
    public DateTime LastStateChange { get; set; }
    
    public bool IsOnline { get; set; }
    
    public DateTime LastSeen { get; set; }
    
    // Navigation properties
    // public ICollection<DeviceGroupMember> DeviceGroupMembers { get; set; } = new List<DeviceGroupMember>();
    public ICollection<DeviceState> States { get; set; } = new List<DeviceState>();
    public ICollection<DeviceEvent> Events { get; set; } = new List<DeviceEvent>();
    public ICollection<DeviceGroup> DeviceGroups { get; set; } = new List<DeviceGroup>();
    public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    
    // New navigation properties for firmware updates and property changes
    public ICollection<DevicePropertyChange> PropertyChanges { get; set; } = new List<DevicePropertyChange>();
    public ICollection<DeviceFirmwareUpdate> FirmwareUpdates { get; set; } = new List<DeviceFirmwareUpdate>();
    
    // Event log navigation property
    public ICollection<DeviceEventLog> EventLogs { get; set; } = new List<DeviceEventLog>();
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               