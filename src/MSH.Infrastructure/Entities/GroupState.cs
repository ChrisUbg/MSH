using System;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class GroupState : BaseEntity
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public JsonDocument State { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
} 