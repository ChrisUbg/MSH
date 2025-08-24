using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Interfaces;

using System.Text.Json;

namespace MSH.Infrastructure.Services;

public class DeviceActionDelayService : IDeviceActionDelayService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceActionDelayService> _logger;
    private readonly IMatterDeviceControlService _matterService;

    public DeviceActionDelayService(
        ApplicationDbContext context, 
        ILogger<DeviceActionDelayService> logger,
        IMatterDeviceControlService matterService)
    {
        _context = context;
        _logger = logger;
        _matterService = matterService;
    }

    // CRUD Operations
    public async Task<DeviceActionDelay> CreateActionDelayAsync(DeviceActionDelay actionDelay)
    {
        actionDelay.Id = Guid.NewGuid();
        actionDelay.CreatedAt = DateTime.UtcNow;
        actionDelay.IsEnabled = true;
        
        // Calculate next execution time
        actionDelay.NextScheduledExecution = DateTime.UtcNow.AddSeconds(actionDelay.DelaySeconds);
        
        _context.DeviceActionDelays.Add(actionDelay);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created action delay for {ActionType} on {Target} with {DelaySeconds}s delay", 
            actionDelay.ActionType, 
            actionDelay.DeviceId != Guid.Empty ? $"Device {actionDelay.DeviceId}" : $"Group {actionDelay.DeviceGroupId}", 
            actionDelay.DelaySeconds);
        
        return actionDelay;
    }

    public async Task<DeviceActionDelay?> GetActionDelayByIdAsync(Guid id)
    {
        return await _context.DeviceActionDelays
            .Include(dad => dad.Device)
            .Include(dad => dad.DeviceGroup)
            .FirstOrDefaultAsync(dad => dad.Id == id);
    }

    public async Task<DeviceActionDelay> UpdateActionDelayAsync(DeviceActionDelay actionDelay)
    {
        actionDelay.UpdatedAt = DateTime.UtcNow;
        
        _context.DeviceActionDelays.Update(actionDelay);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated action delay {Id}", actionDelay.Id);
        return actionDelay;
    }

    public async Task<bool> DeleteActionDelayAsync(Guid id)
    {
        var actionDelay = await _context.DeviceActionDelays.FindAsync(id);
        if (actionDelay == null) return false;
        
        _context.DeviceActionDelays.Remove(actionDelay);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted action delay {Id}", id);
        return true;
    }

    // Device-specific operations
    public async Task<IEnumerable<DeviceActionDelay>> GetActionDelaysForDeviceAsync(Guid deviceId)
    {
        return await _context.DeviceActionDelays
            .Include(dad => dad.Device)
            .Where(dad => dad.DeviceId == deviceId && dad.IsEnabled)
            .OrderBy(dad => dad.NextScheduledExecution)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceActionDelay>> GetActionDelaysForDeviceGroupAsync(Guid deviceGroupId)
    {
        return await _context.DeviceActionDelays
            .Include(dad => dad.DeviceGroup)
            .Where(dad => dad.DeviceGroupId == deviceGroupId && dad.IsEnabled)
            .OrderBy(dad => dad.NextScheduledExecution)
            .ToListAsync();
    }

    // Scheduling operations
    public async Task<IEnumerable<DeviceActionDelay>> GetPendingActionDelaysAsync()
    {
        return await _context.DeviceActionDelays
            .Include(dad => dad.Device)
            .Include(dad => dad.DeviceGroup)
            .Where(dad => dad.IsEnabled && 
                         dad.NextScheduledExecution <= DateTime.UtcNow &&
                         dad.NextScheduledExecution != null)
            .OrderBy(dad => dad.Priority)
            .ThenBy(dad => dad.NextScheduledExecution)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceActionDelay>> GetOverdueActionDelaysAsync()
    {
        var overdueThreshold = DateTime.UtcNow.AddMinutes(-5); // 5 minutes overdue
        return await _context.DeviceActionDelays
            .Include(dad => dad.Device)
            .Include(dad => dad.DeviceGroup)
            .Where(dad => dad.IsEnabled && 
                         dad.NextScheduledExecution <= overdueThreshold &&
                         dad.NextScheduledExecution != null)
            .OrderBy(dad => dad.Priority)
            .ThenBy(dad => dad.NextScheduledExecution)
            .ToListAsync();
    }

    public async Task<bool> ExecuteActionDelayAsync(Guid actionDelayId)
    {
        var actionDelay = await GetActionDelayByIdAsync(actionDelayId);
        if (actionDelay == null) return false;
        
        return await ExecuteActionDelayAsync(actionDelay);
    }

    public async Task<bool> ExecuteActionDelayAsync(DeviceActionDelay actionDelay)
    {
        try
        {
            _logger.LogInformation("Executing action delay {Id}: {ActionType}", actionDelay.Id, actionDelay.ActionType);
            
            // Execute the action based on type
            bool success = false;
            string result = "";
            
            if (actionDelay.DeviceId != Guid.Empty)
            {
                // Execute on individual device
                success = await ExecuteDeviceActionAsync(actionDelay);
            }
            else if (actionDelay.DeviceGroupId.HasValue)
            {
                // Execute on device group
                success = await ExecuteDeviceGroupActionAsync(actionDelay);
            }
            
            // Update execution status
            actionDelay.LastExecuted = DateTime.UtcNow;
            actionDelay.ExecutionResult = success ? "Success" : "Failed";
            
            if (actionDelay.IsRecurring && actionDelay.RecurrenceIntervalSeconds.HasValue)
            {
                // Schedule next execution
                actionDelay.NextScheduledExecution = DateTime.UtcNow.AddSeconds(actionDelay.RecurrenceIntervalSeconds.Value);
            }
            else
            {
                // Disable one-time actions
                actionDelay.IsEnabled = false;
            }
            
            await UpdateActionDelayAsync(actionDelay);
            
            _logger.LogInformation("Action delay {Id} executed with result: {Result}", actionDelay.Id, success ? "Success" : "Failed");
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action delay {Id}", actionDelay.Id);
            actionDelay.ExecutionResult = $"Error: {ex.Message}";
            await UpdateActionDelayAsync(actionDelay);
            return false;
        }
    }

    // Batch operations
    public async Task<bool> ExecuteAllPendingActionDelaysAsync()
    {
        var pendingActions = await GetPendingActionDelaysAsync();
        var results = new List<bool>();
        
        foreach (var action in pendingActions)
        {
            var result = await ExecuteActionDelayAsync(action);
            results.Add(result);
        }
        
        _logger.LogInformation("Executed {Count} pending action delays. Success: {SuccessCount}", 
            pendingActions.Count(), results.Count(r => r));
        
        return results.Any();
    }

    public async Task<bool> ScheduleDeviceActionAsync(Guid deviceId, string actionType, int delaySeconds, string? parameters = null)
    {
        var actionDelay = new DeviceActionDelay
        {
            DeviceId = deviceId,
            ActionType = actionType,
            DelaySeconds = delaySeconds,
            ActionParameters = parameters,
            Priority = 0
        };
        
        await CreateActionDelayAsync(actionDelay);
        return true;
    }

    public async Task<bool> ScheduleDeviceGroupActionAsync(Guid deviceGroupId, string actionType, int delaySeconds, string? parameters = null)
    {
        var actionDelay = new DeviceActionDelay
        {
            DeviceGroupId = deviceGroupId,
            ActionType = actionType,
            DelaySeconds = delaySeconds,
            ActionParameters = parameters,
            Priority = 0
        };
        
        await CreateActionDelayAsync(actionDelay);
        return true;
    }

    // Utility operations
    public async Task<bool> CancelPendingActionAsync(Guid actionDelayId)
    {
        var actionDelay = await GetActionDelayByIdAsync(actionDelayId);
        if (actionDelay == null) return false;
        
        actionDelay.IsEnabled = false;
        await UpdateActionDelayAsync(actionDelay);
        
        _logger.LogInformation("Cancelled pending action delay {Id}", actionDelayId);
        return true;
    }

    public async Task<bool> CancelAllPendingActionsForDeviceAsync(Guid deviceId)
    {
        var actions = await GetActionDelaysForDeviceAsync(deviceId);
        var cancelledCount = 0;
        
        foreach (var action in actions)
        {
            action.IsEnabled = false;
            await UpdateActionDelayAsync(action);
            cancelledCount++;
        }
        
        _logger.LogInformation("Cancelled {Count} pending actions for device {DeviceId}", cancelledCount, deviceId);
        return cancelledCount > 0;
    }

    public async Task<bool> CancelAllPendingActionsForDeviceGroupAsync(Guid deviceGroupId)
    {
        var actions = await GetActionDelaysForDeviceGroupAsync(deviceGroupId);
        var cancelledCount = 0;
        
        foreach (var action in actions)
        {
            action.IsEnabled = false;
            await UpdateActionDelayAsync(action);
            cancelledCount++;
        }
        
        _logger.LogInformation("Cancelled {Count} pending actions for device group {GroupId}", cancelledCount, deviceGroupId);
        return cancelledCount > 0;
    }

    // Status operations
    public async Task<Dictionary<string, object>> GetActionDelayStatusAsync(Guid actionDelayId)
    {
        var actionDelay = await GetActionDelayByIdAsync(actionDelayId);
        if (actionDelay == null) return new Dictionary<string, object>();
        
        return new Dictionary<string, object>
        {
            ["id"] = actionDelay.Id,
            ["actionType"] = actionDelay.ActionType,
            ["delaySeconds"] = actionDelay.DelaySeconds,
            ["isEnabled"] = actionDelay.IsEnabled,
            ["lastExecuted"] = actionDelay.LastExecuted,
            ["nextScheduledExecution"] = actionDelay.NextScheduledExecution,
            ["executionResult"] = actionDelay.ExecutionResult,
            ["isRecurring"] = actionDelay.IsRecurring,
            ["priority"] = actionDelay.Priority
        };
    }

    public async Task<IEnumerable<DeviceActionDelay>> GetRecurringActionDelaysAsync()
    {
        return await _context.DeviceActionDelays
            .Include(dad => dad.Device)
            .Include(dad => dad.DeviceGroup)
            .Where(dad => dad.IsEnabled && dad.IsRecurring)
            .OrderBy(dad => dad.NextScheduledExecution)
            .ToListAsync();
    }

    public async Task<bool> UpdateNextExecutionTimeAsync(Guid actionDelayId)
    {
        var actionDelay = await GetActionDelayByIdAsync(actionDelayId);
        if (actionDelay == null || !actionDelay.IsRecurring || !actionDelay.RecurrenceIntervalSeconds.HasValue)
            return false;
        
        actionDelay.NextScheduledExecution = DateTime.UtcNow.AddSeconds(actionDelay.RecurrenceIntervalSeconds.Value);
        await UpdateActionDelayAsync(actionDelay);
        return true;
    }

    // Private helper methods
    private async Task<bool> ExecuteDeviceActionAsync(DeviceActionDelay actionDelay)
    {
        try
        {
            _logger.LogInformation("Executing {ActionType} on device {DeviceId}", actionDelay.ActionType, actionDelay.DeviceId);
            
            // Get the device to find its MatterDeviceId
            var device = await _context.Devices.FindAsync(actionDelay.DeviceId);
            if (device == null)
            {
                _logger.LogWarning("Device {DeviceId} not found for action {ActionType}", actionDelay.DeviceId, actionDelay.ActionType);
                return false;
            }

            if (string.IsNullOrEmpty(device.MatterDeviceId))
            {
                _logger.LogWarning("Device {DeviceId} has no MatterDeviceId configured", actionDelay.DeviceId);
                return false;
            }

            // Use the injected MatterDeviceControlService to execute the action

            bool result = false;
            switch (actionDelay.ActionType.ToLower())
            {
                case "turn_on":
                case "on":
                    result = await _matterService.TurnOnDeviceAsync(device.MatterDeviceId);
                    break;
                case "turn_off":
                case "off":
                    result = await _matterService.TurnOffDeviceAsync(device.MatterDeviceId);
                    break;
                case "toggle":
                    result = await _matterService.ToggleDeviceAsync(device.MatterDeviceId);
                    break;
                default:
                    _logger.LogWarning("Unknown action type: {ActionType}", actionDelay.ActionType);
                    return false;
            }

            _logger.LogInformation("Action {ActionType} on device {DeviceId} completed with result: {Result}", 
                actionDelay.ActionType, actionDelay.DeviceId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action {ActionType} on device {DeviceId}", 
                actionDelay.ActionType, actionDelay.DeviceId);
            return false;
        }
    }

    private async Task<bool> ExecuteDeviceGroupActionAsync(DeviceActionDelay actionDelay)
    {
        // This would execute the action on all devices in the group
        _logger.LogInformation("Executing {ActionType} on device group {GroupId}", actionDelay.ActionType, actionDelay.DeviceGroupId);
        
        // TODO: Get all devices in the group and execute action on each
        // var group = await _context.DeviceGroups
        //     .Include(g => g.Devices)
        //     .FirstOrDefaultAsync(g => g.Id == actionDelay.DeviceGroupId);
        // 
        // if (group == null) return false;
        // 
        // var results = new List<bool>();
        // foreach (var device in group.Devices)
        // {
        //     var result = await ExecuteDeviceActionAsync(new DeviceActionDelay
        //     {
        //         DeviceId = device.Id,
        //         ActionType = actionDelay.ActionType,
        //         ActionParameters = actionDelay.ActionParameters
        //     });
        //     results.Add(result);
        // }
        // 
        // return results.Any(r => r);
        
        // Simulate success for now
        await Task.Delay(200); // Simulate execution time
        return true;
    }
}
