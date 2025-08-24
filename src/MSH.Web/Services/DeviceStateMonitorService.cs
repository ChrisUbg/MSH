using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MSH.Web.Services
{
    public class DeviceStateMonitorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeviceStateMonitorService> _logger;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(2); // Check every 2 minutes

        public DeviceStateMonitorService(
            IServiceProvider serviceProvider,
            ILogger<DeviceStateMonitorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Device State Monitor Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MonitorDeviceStates(stoppingToken);
                    await Task.Delay(_monitoringInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service is being stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in device state monitoring cycle");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 minute before retrying
                }
            }

            _logger.LogInformation("Device State Monitor Service stopped");
        }

        private async Task MonitorDeviceStates(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deviceStateManager = scope.ServiceProvider.GetRequiredService<IDeviceStateManager>();

            try
            {
                // Get all Matter devices from database
                var matterDevices = await dbContext.Devices
                    .Where(d => !string.IsNullOrEmpty(d.MatterDeviceId))
                    .Select(d => d.MatterDeviceId)
                    .ToListAsync(stoppingToken);

                _logger.LogDebug("Monitoring {Count} Matter devices", matterDevices.Count);

                // Refresh states for all devices in parallel (with limited concurrency)
                var semaphore = new SemaphoreSlim(3, 3); // Max 3 concurrent device checks
                var tasks = matterDevices.Select(async nodeId =>
                {
                    await semaphore.WaitAsync(stoppingToken);
                    try
                    {
                        await deviceStateManager.RefreshDeviceStateAsync(nodeId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to refresh state for device {NodeId}", nodeId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                _logger.LogDebug("Device state monitoring cycle completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during device state monitoring");
            }
        }
    }
}
