using MSH.Commissioning.App.Models;

namespace MSH.Commissioning.App.Services
{
    public interface ICommissioningService
    {
        Task<CommissioningResult> StartCommissioningAsync(CommissioningRequest request, string sessionId);
        Task<CommissioningResult> TestDeviceConnectionAsync(string deviceAddress);
        Task<bool> TransferToPiAsync(string deviceId, string piIp, string piUser);
        event Action<CommissioningProgress>? ProgressUpdated;
    }
}
