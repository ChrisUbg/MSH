using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSH.Infrastructure.Entities;

public class RuleTrigger : BaseEntity
{
    [Required]
    public Guid RuleId { get; set; }

    [Required, MaxLength(500)]
    public string TriggerType { get; set; } = null!;

    [Required]
    public JsonDocument Trigger { get; set; } = null!;

    [Required]
    public bool IsEnabled { get; set; } = true;

    public DateTime? LastTriggered { get; set; }

    [ForeignKey(nameof(RuleId))]
    public virtual Rule Rule { get; set; } = null!;
} 