using System;

namespace MSH.Infrastructure.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "user";
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; } = true;
} 