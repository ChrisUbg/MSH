using System.ComponentModel.DataAnnotations;

namespace MSH.Infrastructure.Entities;

public class User
{
    [MaxLength(500)]
    public String Id { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
    
    [MaxLength(500)]
    public String CreatedById { get; set; } = "bb1be326-f26e-4684-bbf5-5c3df450dc61";
    
    [MaxLength(500)]
    public String? UpdatedById { get; set; } = "bb1be326-f26e-4684-bbf5-5c3df450dc61";
    
    [MaxLength(150)]
    public string UserName { get; set; } = "system";
    
    [MaxLength(150)]
    public string? Email { get; set; }
    
    [MaxLength(150)]
    public string? FirstName { get; set; }
    
    [MaxLength(150)]
    public string? LastName { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
} 