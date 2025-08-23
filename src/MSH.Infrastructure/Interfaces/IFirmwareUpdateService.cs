using MSH.Infrastructure.Entities;
using System.Text.Json;

namespace MSH.Infrastructure.Interfaces;

public interface IFirmwareUpdateService
{
    // Firmware Update Management
    Task<IEnumerable<FirmwareUpdate>> GetAvailableUpdatesAsync();
    Task<FirmwareUpdate?> GetFirmwareUpdateByIdAsync(Guid id);
    Task<FirmwareUpdate> CreateFirmwareUpdateAsync(FirmwareUpdate firmwareUpdate);
    Task<FirmwareUpdate> UpdateFirmwareUpdateAsync(FirmwareUpdate firmwareUpdate);
    Task<bool> DeleteFirmwareUpdateAsync(Guid id);
    
    // Device-Specific Operations
    Task<IEnumerable<DeviceFirmwareUpdate>> GetDeviceUpdatesAsync(Guid deviceId);
    Task<DeviceFirmwareUpdate?> GetDeviceUpdateByIdAsync(Guid id);
    Task<DeviceFirmwareUpdate> CreateDeviceUpdateAsync(DeviceFirmwareUpdate deviceUpdate);
    Task<DeviceFirmwareUpdate> UpdateDeviceUpdateAsync(DeviceFirmwareUpdate deviceUpdate);
    
    // Firmware Discovery
    Task<IEnumerable<FirmwareUpdate>> SearchForUpdatesAsync(Guid deviceId);
    Task<bool> CheckForUpdatesAsync(Guid deviceId);
    
    // Download Operations
    Task<bool> StartDownloadAsync(Guid deviceUpdateId);
    Task<bool> CancelDownloadAsync(Guid deviceUpdateId);
    Task<long> GetDownloadProgressAsync(Guid deviceUpdateId);
    
    // Installation Operations
    Task<bool> StartInstallationAsync(Guid deviceUpdateId);
    Task<bool> CancelInstallationAsync(Guid deviceUpdateId);
    Task<bool> ConfirmInstallationAsync(Guid deviceUpdateId, string confirmedBy);
    
    // Testing Operations
    Task<bool> StartTestingAsync(Guid deviceUpdateId);
    Task<bool> CompleteTestingAsync(Guid deviceUpdateId, bool testPassed, JsonDocument? testResults);
    
    // Rollback Operations
    Task<bool> StartRollbackAsync(Guid deviceUpdateId, string reason);
    Task<bool> CancelRollbackAsync(Guid deviceUpdateId);
    
    // Batch Operations
    Task<IEnumerable<DeviceFirmwareUpdate>> GetPendingUpdatesAsync();
    Task<bool> StartBatchUpdateAsync(IEnumerable<Guid> deviceIds, Guid firmwareUpdateId);
    Task<bool> CancelBatchUpdateAsync(IEnumerable<Guid> deviceIds);
}
