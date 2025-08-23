using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Interfaces;

namespace MSH.Infrastructure.Services;

public class FirmwareUpdateService : IFirmwareUpdateService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FirmwareUpdateService> _logger;

    public FirmwareUpdateService(ApplicationDbContext context, ILogger<FirmwareUpdateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Firmware Update Management
    public async Task<IEnumerable<FirmwareUpdate>> GetAvailableUpdatesAsync()
    {
        return await _context.FirmwareUpdates
            .Where(f => f.Status == "available" && f.IsCompatible)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<FirmwareUpdate?> GetFirmwareUpdateByIdAsync(Guid id)
    {
        return await _context.FirmwareUpdates
            .Include(f => f.DeviceUpdates)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<FirmwareUpdate> CreateFirmwareUpdateAsync(FirmwareUpdate firmwareUpdate)
    {
        firmwareUpdate.Id = Guid.NewGuid();
        firmwareUpdate.CreatedAt = DateTime.UtcNow;
        firmwareUpdate.Status = "available";
        
        _context.FirmwareUpdates.Add(firmwareUpdate);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created firmware update: {Name} v{TargetVersion}", 
            firmwareUpdate.Name, firmwareUpdate.TargetVersion);
        
        return firmwareUpdate;
    }

    public async Task<FirmwareUpdate> UpdateFirmwareUpdateAsync(FirmwareUpdate firmwareUpdate)
    {
        firmwareUpdate.UpdatedAt = DateTime.UtcNow;
        
        _context.FirmwareUpdates.Update(firmwareUpdate);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated firmware update: {Name} v{TargetVersion}", 
            firmwareUpdate.Name, firmwareUpdate.TargetVersion);
        
        return firmwareUpdate;
    }

    public async Task<bool> DeleteFirmwareUpdateAsync(Guid id)
    {
        var firmwareUpdate = await _context.FirmwareUpdates.FindAsync(id);
        if (firmwareUpdate == null) return false;
        
        _context.FirmwareUpdates.Remove(firmwareUpdate);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted firmware update: {Name}", firmwareUpdate.Name);
        return true;
    }

    // Device-Specific Operations
    public async Task<IEnumerable<DeviceFirmwareUpdate>> GetDeviceUpdatesAsync(Guid deviceId)
    {
        return await _context.DeviceFirmwareUpdates
            .Include(du => du.FirmwareUpdate)
            .Where(du => du.DeviceId == deviceId)
            .OrderByDescending(du => du.CreatedAt)
            .ToListAsync();
    }

    public async Task<DeviceFirmwareUpdate?> GetDeviceUpdateByIdAsync(Guid id)
    {
        return await _context.DeviceFirmwareUpdates
            .Include(du => du.FirmwareUpdate)
            .Include(du => du.Device)
            .FirstOrDefaultAsync(du => du.Id == id);
    }

    public async Task<DeviceFirmwareUpdate> CreateDeviceUpdateAsync(DeviceFirmwareUpdate deviceUpdate)
    {
        deviceUpdate.Id = Guid.NewGuid();
        deviceUpdate.CreatedAt = DateTime.UtcNow;
        deviceUpdate.Status = "pending";
        
        _context.DeviceFirmwareUpdates.Add(deviceUpdate);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created device firmware update for device {DeviceId}: {TargetVersion}", 
            deviceUpdate.DeviceId, deviceUpdate.TargetVersion);
        
        return deviceUpdate;
    }

    public async Task<DeviceFirmwareUpdate> UpdateDeviceUpdateAsync(DeviceFirmwareUpdate deviceUpdate)
    {
        deviceUpdate.UpdatedAt = DateTime.UtcNow;
        
        _context.DeviceFirmwareUpdates.Update(deviceUpdate);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated device firmware update {Id}: {Status}", 
            deviceUpdate.Id, deviceUpdate.Status);
        
        return deviceUpdate;
    }

    // Firmware Discovery
    public async Task<IEnumerable<FirmwareUpdate>> SearchForUpdatesAsync(Guid deviceId)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == deviceId);
        
        if (device?.DeviceType == null) return Enumerable.Empty<FirmwareUpdate>();
        
        // Search for compatible firmware updates
        var currentFirmwareVersion = "unknown";
        if (device.DeviceType?.Capabilities != null)
        {
            try
            {
                var capabilities = device.DeviceType.Capabilities.RootElement;
                if (capabilities.TryGetProperty("firmware_version", out var firmwareVersion))
                {
                    currentFirmwareVersion = firmwareVersion.GetString() ?? "unknown";
                }
            }
            catch
            {
                // If there's an error parsing capabilities, use default
                currentFirmwareVersion = "unknown";
            }
        }
        
        return await _context.FirmwareUpdates
            .Where(f => f.Status == "available" && f.IsCompatible)
            .Where(f => f.CurrentVersion != currentFirmwareVersion)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> CheckForUpdatesAsync(Guid deviceId)
    {
        var updates = await SearchForUpdatesAsync(deviceId);
        return updates.Any();
    }

    // Download Operations
    public async Task<bool> StartDownloadAsync(Guid deviceUpdateId)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate == null) return false;
        
        deviceUpdate.Status = "downloading";
        deviceUpdate.DownloadStartedAt = DateTime.UtcNow;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Started download for device update {Id}", deviceUpdateId);
        return true;
    }

    public async Task<bool> CancelDownloadAsync(Guid deviceUpdateId)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate?.Status != "downloading") return false;
        
        deviceUpdate.Status = "pending";
        deviceUpdate.DownloadStartedAt = null;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Cancelled download for device update {Id}", deviceUpdateId);
        return true;
    }

    public async Task<long> GetDownloadProgressAsync(Guid deviceUpdateId)
    {
        // This would typically integrate with a download manager
        // For now, return a placeholder value
        return 0;
    }

    // Installation Operations
    public async Task<bool> StartInstallationAsync(Guid deviceUpdateId)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate?.Status != "downloaded") return false;
        
        deviceUpdate.Status = "installing";
        deviceUpdate.InstallationStartedAt = DateTime.UtcNow;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Started installation for device update {Id}", deviceUpdateId);
        return true;
    }

    public async Task<bool> CancelInstallationAsync(Guid deviceUpdateId)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate?.Status != "installing") return false;
        
        deviceUpdate.Status = "downloaded";
        deviceUpdate.InstallationStartedAt = null;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Cancelled installation for device update {Id}", deviceUpdateId);
        return true;
    }

    public async Task<bool> ConfirmInstallationAsync(Guid deviceUpdateId, string confirmedBy)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate?.Status != "completed") return false;
        
        deviceUpdate.IsConfirmed = true;
        deviceUpdate.ConfirmedAt = DateTime.UtcNow;
        deviceUpdate.ConfirmedBy = confirmedBy;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Confirmed installation for device update {Id} by {User}", 
            deviceUpdateId, confirmedBy);
        return true;
    }

    // Testing Operations
    public async Task<bool> StartTestingAsync(Guid deviceUpdateId)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate?.Status != "completed") return false;
        
        // Update status to indicate testing is in progress
        deviceUpdate.TestPassed = false;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Started testing for device update {Id}", deviceUpdateId);
        return true;
    }

    public async Task<bool> CompleteTestingAsync(Guid deviceUpdateId, bool testPassed, JsonDocument? testResults)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate == null) return false;
        
        deviceUpdate.TestPassed = testPassed;
        deviceUpdate.TestResults = testResults;
        deviceUpdate.TestCompletedAt = DateTime.UtcNow;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Completed testing for device update {Id}: {Result}", 
            deviceUpdateId, testPassed ? "PASSED" : "FAILED");
        return true;
    }

    // Rollback Operations
    public async Task<bool> StartRollbackAsync(Guid deviceUpdateId, string reason)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate?.Status != "completed" && deviceUpdate?.Status != "failed") return false;
        
        deviceUpdate.Status = "rolled_back";
        deviceUpdate.RollbackReason = reason;
        deviceUpdate.RollbackCompletedAt = DateTime.UtcNow;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Started rollback for device update {Id}: {Reason}", 
            deviceUpdateId, reason);
        return true;
    }

    public async Task<bool> CancelRollbackAsync(Guid deviceUpdateId)
    {
        var deviceUpdate = await GetDeviceUpdateByIdAsync(deviceUpdateId);
        if (deviceUpdate?.Status != "rolled_back") return false;
        
        deviceUpdate.Status = "completed";
        deviceUpdate.RollbackReason = null;
        deviceUpdate.RollbackCompletedAt = null;
        
        await UpdateDeviceUpdateAsync(deviceUpdate);
        
        _logger.LogInformation("Cancelled rollback for device update {Id}", deviceUpdateId);
        return true;
    }

    // Batch Operations
    public async Task<IEnumerable<DeviceFirmwareUpdate>> GetPendingUpdatesAsync()
    {
        return await _context.DeviceFirmwareUpdates
            .Include(du => du.FirmwareUpdate)
            .Include(du => du.Device)
            .Where(du => du.Status == "pending")
            .OrderBy(du => du.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> StartBatchUpdateAsync(IEnumerable<Guid> deviceIds, Guid firmwareUpdateId)
    {
        var firmwareUpdate = await GetFirmwareUpdateByIdAsync(firmwareUpdateId);
        if (firmwareUpdate == null) return false;
        
        var deviceUpdates = new List<DeviceFirmwareUpdate>();
        
        foreach (var deviceId in deviceIds)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null) continue;
            
            var currentVersion = "unknown";
            if (device.Properties != null)
            {
                try
                {
                    var properties = device.Properties.RootElement;
                    if (properties.TryGetProperty("firmware_version", out var firmwareVersion))
                    {
                        currentVersion = firmwareVersion.GetString() ?? "unknown";
                    }
                }
                catch
                {
                    // If there's an error parsing properties, use default
                    currentVersion = "unknown";
                }
            }
            
            var deviceUpdate = new DeviceFirmwareUpdate
            {
                DeviceId = deviceId,
                FirmwareUpdateId = firmwareUpdateId,
                CurrentVersion = currentVersion,
                TargetVersion = firmwareUpdate.TargetVersion,
                Status = "pending"
            };
            
            deviceUpdates.Add(deviceUpdate);
        }
        
        _context.DeviceFirmwareUpdates.AddRange(deviceUpdates);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Started batch update for {Count} devices with firmware {FirmwareId}", 
            deviceUpdates.Count, firmwareUpdateId);
        
        return true;
    }

    public async Task<bool> CancelBatchUpdateAsync(IEnumerable<Guid> deviceIds)
    {
        var deviceUpdates = await _context.DeviceFirmwareUpdates
            .Where(du => deviceIds.Contains(du.DeviceId) && du.Status == "pending")
            .ToListAsync();
        
        foreach (var deviceUpdate in deviceUpdates)
        {
            deviceUpdate.Status = "cancelled";
            deviceUpdate.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Cancelled batch update for {Count} devices", deviceUpdates.Count);
        return true;
    }
}
