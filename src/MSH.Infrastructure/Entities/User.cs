using System;

namespace MSH.Infrastructure.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; } = "system";
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
} 