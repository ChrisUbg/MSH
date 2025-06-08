using System;
using System.Threading.Tasks;

namespace MSH.Web.Services;

public interface IBackupService
{
    /// <summary>
    /// Creates a backup of the database
    /// </summary>
    /// <param name="backupPath">Optional path where to store the backup. If not provided, uses default location.</param>
    /// <returns>The path to the created backup file</returns>
    Task<string> CreateBackupAsync(string? backupPath = null);

    /// <summary>
    /// Restores the database from a backup file
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    Task RestoreBackupAsync(string backupPath);

    /// <summary>
    /// Lists all available backups
    /// </summary>
    /// <returns>Array of backup file paths</returns>
    Task<string[]> ListBackupsAsync();

    /// <summary>
    /// Deletes old backups based on retention policy
    /// </summary>
    /// <param name="maxAgeInDays">Maximum age of backups to keep</param>
    Task CleanupOldBackupsAsync(int maxAgeInDays = 30);

    /// <summary>
    /// Gets the default backup directory path
    /// </summary>
    string GetDefaultBackupDirectory();
} 