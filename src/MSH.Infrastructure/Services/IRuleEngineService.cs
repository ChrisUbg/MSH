using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public interface IRuleEngineService
{
    Task EvaluateRuleAsync(Rule rule);
    Task TriggerRuleAsync(RuleTrigger trigger);
    Task ExecuteActionAsync(Rule rule);
} 