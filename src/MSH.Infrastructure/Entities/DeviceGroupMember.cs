namespace MSH.Infrastructure.Entities;

public class DeviceGroupMember : BaseEntity
{
    public int GroupId { get; set; }
    public int DeviceId { get; set; }
    
    // Navigation properties
    public DeviceGroup Group { get; set; } = null!;
    public Device Device { get; set; } = null!;
} 