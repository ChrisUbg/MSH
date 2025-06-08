using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSH.Web.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MSH.Web.Services;

public class BackupBackgroundService : BackgroundService
{
    private readonly ILogger<BackupBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _backupPath;

    public BackupBackgroundService(
        ILogger<BackupBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        
        if (!Directory.Exists(_backupPath))
        {
            Directory.CreateDirectory(_backupPath);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformBackup();
                // Wait for 24 hours before next backup
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while performing backup");
                // Wait for 1 hour before retrying on error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task PerformBackup()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"backup_{timestamp}.sql";
        var backupFilePath = Path.Combine(_backupPath, backupFileName);

        // Get the connection string from the context
        var connectionString = dbContext.Database.GetConnectionString();
        
        // Perform the backup using pg_dump
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pg_dump",
            Arguments = $"-Fc \"{connectionString}\" -f \"{backupFilePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception("Failed to start backup process.");
        }
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Backup failed with exit code {process.ExitCode}");
        }

        _logger.LogInformation("Backup completed successfully: {BackupFile}", backupFilePath);

        // Clean up old backups (keep last 7 days)
        CleanupOldBackups();
    }

    private void CleanupOldBackups()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var backupFiles = Directory.GetFiles(_backupPath, "backup_*.sql");

        foreach (var file in backupFiles)
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.CreationTimeUtc < cutoffDate)
            {
                try
                {
                    fileInfo.Delete();
                    _logger.LogInformation("Deleted old backup file: {BackupFile}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting old backup file: {BackupFile}", file);
                }
            }
        }
    }
} 