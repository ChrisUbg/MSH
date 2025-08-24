using MSH.Commissioning.App.Models;

namespace MSH.Commissioning.App.Services
{
    public interface IBLEScannerService
    {
        Task<bool> IsBluetoothAvailableAsync();
        Task<List<BLEDevice>> ScanForDevicesAsync(int timeoutSeconds = 30);
        Task<BLEDevice?> GetDeviceInfoAsync(string deviceAddress);
        Task<bool> ConnectToDeviceAsync(string deviceAddress);
        Task<bool> DisconnectFromDeviceAsync(string deviceAddress);
        event Action<BLEDevice>? DeviceDiscovered;
        event Action<string>? ScanError;
    }
}
