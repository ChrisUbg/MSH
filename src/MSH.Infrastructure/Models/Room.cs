namespace MSH.Infrastructure.Models;

public class Room
{
    public int RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Location { get; set; }
    public ICollection<Device>? Devices { get; set; }
} 