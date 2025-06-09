using System;

namespace MSH.Infrastructure.Models;

public class mGroupMember
{
    public string GroupId { get; set; } = null!;
    public Guid DeviceId { get; set; }
    public DateTime AddedAt { get; set; }
    public string? AddedBy { get; set; }
    public string? Role { get; set; }
    public string? Metadata { get; set; }

    public virtual mGroup Group { get; set; } = null!;
    public virtual mDevice Device { get; set; } = null!;
} 