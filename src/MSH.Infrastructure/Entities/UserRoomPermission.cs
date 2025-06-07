namespace MSH.Infrastructure.Entities;

public class UserRoomPermission : BaseEntity
{
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public string PermissionLevel { get; set; } = null!; // e.g., 'read', 'write', 'admin'
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
} 