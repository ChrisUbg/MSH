using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class GroupStateHistory : BaseEntity
{
    public Guid GroupId { get; set; }
    
    public Group Group { get; set; } = null!;
    public JsonDocument OldState { get; set; } = null!;
    public JsonDocument NewState { get; set; } = null!;
    
    [MaxLength(150)]
    public string? Description { get; set; }
} 