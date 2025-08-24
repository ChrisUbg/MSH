using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Interfaces;

public interface IDeviceActionDelayService
{
    // CRUD Operations
    Task<DeviceActionDelay> CreateActionDelayAsync(DeviceActionDelay actionDelay);
    Task<DeviceActionDelay?> GetActionDelayByIdAsync(Guid id);
    Task<DeviceActionDelay> UpdateActionDelayAsync(DeviceActionDelay actionDelay);
    Task<bool> DeleteActionDelayAsync(Guid id);
    
    // Device-specific operations
    Task<IEnumerable<DeviceActionDelay>> GetActionDelaysForDeviceAsync(Guid deviceId);
    Task<IEnumerable<DeviceActionDelay>> GetActionDelaysForDeviceGroupAsync(Guid deviceGroupId);
    
    // Scheduling operations
    Task<IEnumerable<DeviceActionDelay>> GetPendingActionDelaysAsync();
    Task<IEnumerable<DeviceActionDelay>> GetOverdueActionDelaysAsync();
    Task<bool> ExecuteActionDelayAsync(Guid actionDelayId);
    Task<bool> ExecuteActionDelayAsync(DeviceActionDelay actionDelay);
    
    // Batch operations
    Task<bool> ExecuteAllPendingActionDelaysAsync();
    Task<bool> ScheduleDeviceActionAsync(Guid deviceId, string actionType, int delaySeconds, string? parameters = null);
    Task<bool> ScheduleDeviceGroupActionAsync(Guid deviceGroupId, string actionType, int delaySeconds, string? parameters = null);
    
    // Utility operations
    Task<bool> CancelPendingActionAsync(Guid actionDelayId);
    Task<bool> CancelAllPendingActionsForDeviceAsync(Guid deviceId);
    Task<bool> CancelAllPendingActionsForDeviceGroupAsync(Guid deviceGroupId);
    
    // Status operations
    Task<Dictionary<string, object>> GetActionDelayStatusAsync(Guid actionDelayId);
    Task<IEnumerable<DeviceActionDelay>> GetRecurringActionDelaysAsync();
    Task<bool> UpdateNextExecutionTimeAsync(Guid actionDelayId);
}
