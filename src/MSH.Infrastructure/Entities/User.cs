using System;

namespace MSH.Infrastructure.Entities;

public class User : BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserName { get; set; } = "defaultuser";
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
} 