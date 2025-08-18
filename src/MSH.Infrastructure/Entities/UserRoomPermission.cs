using System.ComponentModel.DataAnnotations;

namespace MSH.Infrastructure.Entities;

public class UserRoomPermission : BaseEntity
{
    [MaxLength(500)]
    public string UserId { get; set; } = null!;
    
    public Guid RoomId { get; set; }
    
    [MaxLength(150)]
    public string Permission { get; set; } = null!; // e.g., 'read', 'write', 'admin'
    
    // Navigation properties
    public User User { get; set; } = null!;
    
    public Room Room { get; set; } = null!;
} 