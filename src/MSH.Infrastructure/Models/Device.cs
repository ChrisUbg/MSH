namespace MSH.Infrastructure.Models;

public class mDevice
{
    public Guid DeviceId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Configuration { get; set; }
    public string State { get; set; }
    public string GroupId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdated { get; set; }
    public DateTime? LastStateChange { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? RoomId { get; set; }
    public mRoom? Room { get; set; }

    public mDevice()
    {
        DeviceId = Guid.Empty;
        Name = string.Empty;
        Type = string.Empty;
        Configuration = string.Empty;
        State = string.Empty;
        GroupId = string.Empty;
    }
} 