using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public interface IGroupStateManager
{
    Task<bool> SynchronizeGroupStateAsync(Guid groupId);
    Task<bool> ValidateGroupStateAsync(Guid groupId, Dictionary<string, object> state);
    Task<bool> PersistGroupStateAsync(Guid groupId, Dictionary<string, object> state);
    Task NotifyStateChangeAsync(Guid groupId, Dictionary<string, object> oldState, Dictionary<string, object> newState);
    Task<Dictionary<string, object>> GetGroupStateHistoryAsync(Guid groupId, int limit = 10);
} 