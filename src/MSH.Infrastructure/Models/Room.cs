using System.Collections.Generic;

namespace MSH.Infrastructure.Models;

public class mRoom
{
    public Guid RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Location { get; set; }
    public ICollection<mDevice>? Devices { get; set; }
} 