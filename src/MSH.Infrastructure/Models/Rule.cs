using System;
using System.Collections.Generic;

namespace MSH.Infrastructure.Models;

public class mRule
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Metadata { get; set; }

    public virtual ICollection<mRuleCondition> Conditions { get; set; } = new List<mRuleCondition>();
    public virtual ICollection<mRuleAction> Actions { get; set; } = new List<mRuleAction>();
    public virtual ICollection<mRuleTrigger> Triggers { get; set; } = new List<mRuleTrigger>();
    public virtual ICollection<mRuleExecutionHistory> ExecutionHistory { get; set; } = new List<mRuleExecutionHistory>();
} 