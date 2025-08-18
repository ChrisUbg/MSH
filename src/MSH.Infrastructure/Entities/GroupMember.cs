namespace MSH.Infrastructure.Entities;

public class GroupMember : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid DeviceId { get; set; }
    
    public Group Group { get; set; } = null!;
    
    public Device Device { get; set; } = null!;
} 