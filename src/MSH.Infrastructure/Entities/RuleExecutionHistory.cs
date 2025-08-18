using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class RuleExecutionHistory : BaseEntity
{
    public Guid RuleId { get; set; }
    
    public Rule Rule { get; set; } = null!;
    
    public bool Success { get; set; }
    public JsonDocument? Result { get; set; }
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    public DateTime ExecutionTime { get; set; }
} 