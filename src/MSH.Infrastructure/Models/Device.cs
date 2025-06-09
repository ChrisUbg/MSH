namespace MSH.Infrastructure.Models;

public class Device
{
    public string DeviceId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Configuration { get; set; }
    public string State { get; set; }
    public string GroupId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdated { get; set; }
    public DateTime? LastStateChange { get; set; }
    public bool IsActive { get; set; } = true;
    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public Device()
    {
        DeviceId = string.Empty;
        Name = string.Empty;
        Type = string.Empty;
        Configuration = string.Empty;
        State = string.Empty;
        GroupId = string.Empty;
    }
} 