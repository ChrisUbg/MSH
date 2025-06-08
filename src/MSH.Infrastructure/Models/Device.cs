namespace MSH.Infrastructure.Models;

public class Device
{
    public string DeviceId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdated { get; set; }
    public bool IsActive { get; set; } = true;
    public int? RoomId { get; set; }
    public Room? Room { get; set; }
} 