using System;

namespace MSH.Infrastructure.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedById { get; set; }
    public int? UpdatedById { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public User? UpdatedBy { get; set; }
} 