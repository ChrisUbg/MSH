using System.ComponentModel.DataAnnotations;

namespace MSH.Infrastructure.Entities;

public class Group : BaseEntity
{
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    
    [MaxLength(150)]
    public string? Description { get; set; }
    
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    
    public GroupState? State { get; set; }
} 