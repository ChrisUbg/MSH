using System;

namespace MSH.Infrastructure.Entities;

public class UserRoomPermission : BaseEntity
{
    public string UserId { get; set; } = null!;
    public Guid RoomId { get; set; }
    public string Permission { get; set; } = null!; // e.g., 'read', 'write', 'admin'
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
} 