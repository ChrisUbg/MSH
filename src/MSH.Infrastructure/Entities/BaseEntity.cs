using System;

namespace MSH.Infrastructure.Entities;

public abstract class BaseEntity : IUserTrackable
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public String CreatedById { get; set; } = "bb1be326-f26e-4684-bbf5-5c3df450dc61";
    public String? UpdatedById { get; set; } = "bb1be326-f26e-4684-bbf5-5c3df450dc61";
} 