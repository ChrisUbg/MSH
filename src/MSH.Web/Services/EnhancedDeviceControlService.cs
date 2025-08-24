using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Interfaces;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Web.Services;

namespace MSH.Web.Services;

public interface IEnhancedDeviceControlService
{
    Task<(bool Success, bool WasDelayed)> ToggleDeviceAsync(string nodeId, Guid deviceId);
    Task<(bool Success, bool WasDelayed)> TurnOnDeviceAsync(string nodeId, Guid deviceId);
    Task<(bool Success, bool WasDelayed)> TurnOffDeviceAsync(string nodeId, Guid deviceId);
    Task<string?> GetDeviceStateAsync(string nodeId);
    Task<bool> IsDeviceOnlineAsync(string nodeId);
    Task<PowerMetricsResult?> GetPowerMetricsAsync(string nodeId);
}

public class EnhancedDeviceControlService : IEnhancedDeviceControlService
{
    private readonly IMatterDeviceControlService _matterService;
    private readonly IDeviceEventDelayService _eventDelayService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EnhancedDeviceControlService> _logger;

    public EnhancedDeviceControlService(
        IMatterDeviceControlService matterService,
        IDeviceEventDelayService eventDelayService,
        ApplicationDbContext context,
        ILogger<EnhancedDeviceControlService> logger)
    {
        _matterService = matterService;
        _eventDelayService = eventDelayService;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, bool WasDelayed)> ToggleDeviceAsync(string nodeId, Guid deviceId)
    {
        Console.WriteLine($"=== ENHANCED DEVICE CONTROL SERVICE ===");
        Console.WriteLine($"ToggleDeviceAsync called for nodeId: {nodeId}, deviceId: {deviceId}");
        
        // First, get the current state to determine if we're turning on or off
        Console.WriteLine($"Getting current device state for nodeId: {nodeId}");
        var currentState = await _matterService.GetDeviceStateAsync(nodeId);
        Console.WriteLine($"Current state returned: {currentState}");
        
        var isCurrentlyOn = !string.IsNullOrEmpty(currentState) && currentState.ToLower() == "on";
        Console.WriteLine($"Is currently on: {isCurrentlyOn}");
        
        // Determine the event type based on current state
        var eventType = isCurrentlyOn ? "turn_off" : "turn_on";
        var actionDescription = isCurrentlyOn ? "Turn off device" : "Turn on device";
        
        Console.WriteLine($"Event type: {eventType}, Action description: {actionDescription}");
        _logger.LogInformation("Toggle device {DeviceId} - current state: {CurrentState}, will execute: {EventType}", 
            deviceId, currentState, eventType);
        
        // Execute the appropriate action based on current state
        var result = isCurrentlyOn 
            ? await ExecuteWithEventDelayAsync(
                () => _matterService.TurnOffDeviceAsync(nodeId),
                deviceId,
                eventType,
                actionDescription
              )
            : await ExecuteWithEventDelayAsync(
                () => _matterService.TurnOnDeviceAsync(nodeId),
                deviceId,
                eventType,
                actionDescription
              );
        
        Console.WriteLine($"ExecuteWithEventDelayAsync returned: {result}");
        Console.WriteLine($"=== ENHANCED DEVICE CONTROL SERVICE COMPLETED ===");
        
        return result;
    }

    public async Task<(bool Success, bool WasDelayed)> TurnOnDeviceAsync(string nodeId, Guid deviceId)
    {
        return await ExecuteWithEventDelayAsync(
            () => _matterService.TurnOnDeviceAsync(nodeId),
            deviceId,
            "turn_on",
            "Turn on device"
        );
    }

    public async Task<(bool Success, bool WasDelayed)> TurnOffDeviceAsync(string nodeId, Guid deviceId)
    {
        return await ExecuteWithEventDelayAsync(
            () => _matterService.TurnOffDeviceAsync(nodeId),
            deviceId,
            "turn_off",
            "Turn off device"
        );
    }

    public async Task<string?> GetDeviceStateAsync(string nodeId)
    {
        return await _matterService.GetDeviceStateAsync(nodeId);
    }

    public async Task<bool> IsDeviceOnlineAsync(string nodeId)
    {
        return await _matterService.IsDeviceOnlineAsync(nodeId);
    }

    public async Task<PowerMetricsResult?> GetPowerMetricsAsync(string nodeId)
    {
        return await _matterService.GetPowerMetricsAsync(nodeId);
    }

    private async Task<(bool Success, bool WasDelayed)> ExecuteWithEventDelayAsync(
        Func<Task<bool>> action,
        Guid deviceId,
        string eventType,
        string actionDescription)
    {
        LoggingConfig.LogEventDelay($"ExecuteWithEventDelayAsync called for device {deviceId}, eventType: {eventType}");
        
        try
        {
            // Check if there are active event delays for this event type
            var eventDelays = await _eventDelayService.GetActiveEventDelaysForEventTypeAsync(deviceId, eventType);
            var deviceEventDelays = eventDelays as DeviceEventDelay[] ?? eventDelays.ToArray();
            LoggingConfig.LogEventDelay($"Found {deviceEventDelays.Count()} event delays for {eventType} on device {deviceId}");
            
            if (deviceEventDelays.Length == 0)
            {
                // No delays configured, execute immediately
                LoggingConfig.LogEventDelay($"No event delays found for {eventType} on device {deviceId}, executing immediately");
                _logger.LogInformation("No event delays found for {EventType} on device {DeviceId}, executing immediately", 
                    eventType, deviceId);
                var result = await action();
                return (result, false); // Not delayed
            }

            // Get the highest priority delay (lowest number = highest priority)
            var highestPriorityDelay = deviceEventDelays.OrderBy(d => d.Priority).First();
            
            LoggingConfig.LogEventDelay($"Event delay found for {eventType} on device {deviceId}: {highestPriorityDelay.DelaySeconds}s delay (Priority: {highestPriorityDelay.Priority})");
            _logger.LogInformation("Event delay found for {EventType} on device {DeviceId}: {DelaySeconds}s delay (Priority: {Priority})", 
                eventType, deviceId, highestPriorityDelay.DelaySeconds, highestPriorityDelay.Priority);

            // Create a one-time action delay for this event
            var actionDelay = new MSH.Infrastructure.Entities.DeviceActionDelay
            {
                DeviceId = deviceId,
                ActionType = eventType,
                DelaySeconds = highestPriorityDelay.DelaySeconds,
                ActionParameters = "{}", // No additional parameters for basic events
                IsEnabled = true,
                IsRecurring = false,
                Priority = highestPriorityDelay.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61", // Admin user ID
                UpdatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61",
                NextScheduledExecution = DateTime.UtcNow.AddSeconds(highestPriorityDelay.DelaySeconds),
                ExecuteOnStartup = false
            };

            // Save the delayed action directly to the database
            LoggingConfig.LogEventDelay($"Saving action delay to database for device {deviceId}");
            _context.DeviceActionDelays.Add(actionDelay);
            await _context.SaveChangesAsync();

            LoggingConfig.LogEventDelay($"Created delayed action for {eventType} on device {deviceId}, will execute in {highestPriorityDelay.DelaySeconds} seconds");
            _logger.LogInformation("Created delayed action for {EventType} on device {DeviceId}, will execute in {DelaySeconds} seconds", 
                eventType, deviceId, highestPriorityDelay.DelaySeconds);

            // Return success immediately (the action will be executed later by the background service)
            return (true, true); // Success, but delayed
        }
        catch (Exception ex)
        {
            LoggingConfig.LogEventDelay($"Error applying event delay for {eventType} on device {deviceId}: {ex.Message}");
            _logger.LogError(ex, "Error applying event delay for {EventType} on device {DeviceId}", eventType, deviceId);
            
            // If there's an error with the delay system, fall back to immediate execution
            LoggingConfig.LogEventDelay($"Falling back to immediate execution for {eventType} on device {deviceId}");
            _logger.LogWarning("Falling back to immediate execution for {EventType} on device {DeviceId}", eventType, deviceId);
            var result = await action();
            return (result, false); // Not delayed
        }
    }
}
