using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Interfaces;

namespace MSH.Infrastructure.Services;

public class DeviceActionDelayBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceActionDelayBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); // Check every 30 seconds

    public DeviceActionDelayBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DeviceActionDelayBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Device Action Delay Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingActionDelaysAsync();
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Device Action Delay Background Service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait longer on error
            }
        }

        _logger.LogInformation("Device Action Delay Background Service stopped");
    }

    private async Task ProcessPendingActionDelaysAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var deviceActionDelayService = scope.ServiceProvider.GetRequiredService<IDeviceActionDelayService>();

        try
        {
            // Get overdue actions first
            var overdueActions = await deviceActionDelayService.GetOverdueActionDelaysAsync();
            if (overdueActions.Any())
            {
                _logger.LogWarning("Found {Count} overdue action delays", overdueActions.Count());
            }

            // Execute all pending actions
            var success = await deviceActionDelayService.ExecuteAllPendingActionDelaysAsync();
            if (success)
            {
                _logger.LogDebug("Successfully processed pending action delays");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending action delays");
        }
    }
}
