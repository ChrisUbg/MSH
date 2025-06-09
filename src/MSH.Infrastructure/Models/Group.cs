using System;
using System.Collections.Generic;

namespace MSH.Infrastructure.Models;

public class mGroup
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public string? Metadata { get; set; }

    public virtual ICollection<mGroupMember> Members { get; set; } = new List<mGroupMember>();
    public virtual mGroupState? State { get; set; }
    public virtual ICollection<mGroupStateHistory> StateHistory { get; set; } = new List<mGroupStateHistory>();
} 