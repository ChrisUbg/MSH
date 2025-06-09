using System.Threading.Tasks;
using System.Collections.Generic;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public interface IGroupStateManager
{
    Task<bool> SynchronizeGroupStateAsync(int groupId);
    Task<bool> ValidateGroupStateAsync(int groupId, Dictionary<string, object> state);
    Task<bool> PersistGroupStateAsync(int groupId, Dictionary<string, object> state);
    Task NotifyStateChangeAsync(int groupId, Dictionary<string, object> oldState, Dictionary<string, object> newState);
    Task<Dictionary<string, object>> GetGroupStateHistoryAsync(int groupId, int limit = 10);
} 