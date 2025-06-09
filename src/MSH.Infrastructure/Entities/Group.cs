using System;
using System.Collections.Generic;

namespace MSH.Infrastructure.Entities;

public class Group : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public GroupState? State { get; set; }
} 