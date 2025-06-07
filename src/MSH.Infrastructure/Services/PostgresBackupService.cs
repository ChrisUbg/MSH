using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MSH.Infrastructure.Services;

public class PostgresBackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PostgresBackupService> _logger;
    private readonly string _backupDirectory;
    private readonly string _host;
    private readonly string _port;
    private readonly string _database;
    private readonly string _username;
    private readonly string _password;

    public PostgresBackupService(
        IConfiguration configuration,
        ILogger<PostgresBackupService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Get PostgreSQL connection details from configuration
        _host = _configuration["ConnectionStrings:DefaultConnection:Host"] ?? "localhost";
        _port = _configuration["ConnectionStrings:DefaultConnection:Port"] ?? "5432";
        _database = _configuration["ConnectionStrings:DefaultConnection:Database"] ?? "msh";
        _username = _configuration["ConnectionStrings:DefaultConnection:Username"] ?? "postgres";
        _password = _configuration["ConnectionStrings:DefaultConnection:Password"] ?? "postgres";

        // Set up backup directory
        _backupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MSH",
            "Backups"
        );

        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public string GetDefaultBackupDirectory() => _backupDirectory;

    public async Task<string> CreateBackupAsync(string? backupPath = null)
    {
        try
        {
            backupPath ??= Path.Combine(
                _backupDirectory,
                $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql"
            );

            var pgDumpPath = "pg_dump"; // Assuming pg_dump is in PATH
            var arguments = $"-h {_host} -p {_port} -U {_username} -F c -b -v -f \"{backupPath}\" {_database}";

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pgDumpPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Set PGPASSWORD environment variable
            startInfo.EnvironmentVariables["PGPASSWORD"] = _password;

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Failed to start pg_dump process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"pg_dump failed with exit code {process.ExitCode}. Error: {error}");
            }

            _logger.LogInformation("Backup created successfully at {BackupPath}", backupPath);
            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup");
            throw;
        }
    }

    public async Task RestoreBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found", backupPath);
            }

            var pgRestorePath = "pg_restore"; // Assuming pg_restore is in PATH
            var arguments = $"-h {_host} -p {_port} -U {_username} -d {_database} -v \"{backupPath}\"";

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pgRestorePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Set PGPASSWORD environment variable
            startInfo.EnvironmentVariables["PGPASSWORD"] = _password;

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Failed to start pg_restore process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"pg_restore failed with exit code {process.ExitCode}. Error: {error}");
            }

            _logger.LogInformation("Backup restored successfully from {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore backup");
            throw;
        }
    }

    public Task<string[]> ListBackupsAsync()
    {
        try
        {
            var backups = Directory.GetFiles(_backupDirectory, "backup_*.sql")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .ToArray();

            return Task.FromResult(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list backups");
            throw;
        }
    }

    public async Task CleanupOldBackupsAsync(int maxAgeInDays = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-maxAgeInDays);
            var backups = await ListBackupsAsync();

            foreach (var backup in backups)
            {
                var fileInfo = new FileInfo(backup);
                if (fileInfo.LastWriteTimeUtc < cutoffDate)
                {
                    File.Delete(backup);
                    _logger.LogInformation("Deleted old backup: {BackupPath}", backup);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old backups");
            throw;
        }
    }
} 