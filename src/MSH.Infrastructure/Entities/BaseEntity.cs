using System;

namespace MSH.Infrastructure.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public User? UpdatedBy { get; set; }
} 