using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MSH.Infrastructure.Services;

namespace MSH.Web.Services;

public class BackupBackgroundService : BackgroundService
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupBackgroundService> _logger;
    private readonly TimeSpan _backupInterval = TimeSpan.FromHours(24); // Daily backup

    public BackupBackgroundService(
        IBackupService backupService,
        ILogger<BackupBackgroundService> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting scheduled backup");
                await _backupService.CreateBackupAsync(null);
                _logger.LogInformation("Scheduled backup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while performing scheduled backup");
            }

            try
            {
                await Task.Delay(_backupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown, exit gracefully
                break;
            }
        }
    }
} 