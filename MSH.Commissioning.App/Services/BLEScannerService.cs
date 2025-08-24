using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MSH.Commissioning.App.Models;

namespace MSH.Commissioning.App.Services
{
    public class BLEScannerService : IBLEScannerService
    {
        private readonly ILogger<BLEScannerService> _logger;

        public event Action<BLEDevice>? DeviceDiscovered;
        public event Action<string>? ScanError;

        public BLEScannerService(ILogger<BLEScannerService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsBluetoothAvailableAsync()
        {
            try
            {
                // Check if bluetoothctl is available
                var result = await RunCommandAsync("bluetoothctl", "--version");
                if (!result.Success)
                {
                    _logger.LogWarning("bluetoothctl not found");
                    return false;
                }

                // Check if any Bluetooth adapter is available
                result = await RunCommandAsync("bluetoothctl", "list");
                if (!result.Success)
                {
                    _logger.LogWarning("No Bluetooth adapters found");
                    return false;
                }

                // Check if output contains "Controller"
                if (!result.Output.Contains("Controller"))
                {
                    _logger.LogWarning("No Bluetooth controllers found");
                    return false;
                }

                _logger.LogInformation("Bluetooth is available");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Bluetooth availability");
                return false;
            }
        }

        public async Task<List<BLEDevice>> ScanForDevicesAsync(int timeoutSeconds = 30)
        {
            var devices = new List<BLEDevice>();
            
            try
            {
                if (!await IsBluetoothAvailableAsync())
                {
                    throw new InvalidOperationException("Bluetooth not available");
                }

                _logger.LogInformation("Starting Bluetooth scan for {Timeout} seconds", timeoutSeconds);

                // Start scanning
                var startResult = await RunCommandAsync("bluetoothctl", "scan on");
                if (!startResult.Success)
                {
                    throw new InvalidOperationException($"Failed to start scanning: {startResult.Error}");
                }

                // Wait for scan duration
                await Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));

                // Stop scanning
                var stopResult = await RunCommandAsync("bluetoothctl", "scan off");
                if (!stopResult.Success)
                {
                    _logger.LogWarning("Failed to stop scanning: {Error}", stopResult.Error);
                }

                // Get discovered devices
                var devicesResult = await RunCommandAsync("bluetoothctl", "devices");
                if (devicesResult.Success)
                {
                    devices = ParseDevices(devicesResult.Output);
                    _logger.LogInformation("Found {Count} devices", devices.Count);
                }
                else
                {
                    throw new InvalidOperationException($"Failed to get devices: {devicesResult.Error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning for devices");
                ScanError?.Invoke(ex.Message);
            }

            return devices;
        }

        public async Task<BLEDevice?> GetDeviceInfoAsync(string deviceAddress)
        {
            try
            {
                var result = await RunCommandAsync("bluetoothctl", $"info {deviceAddress}");
                if (result.Success)
                {
                    return ParseDeviceInfo(deviceAddress, result.Output);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device info for {Address}", deviceAddress);
            }

            return null;
        }

        public async Task<bool> ConnectToDeviceAsync(string deviceAddress)
        {
            try
            {
                var result = await RunCommandAsync("bluetoothctl", $"connect {deviceAddress}");
                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to device {Address}", deviceAddress);
                return false;
            }
        }

        public async Task<bool> DisconnectFromDeviceAsync(string deviceAddress)
        {
            try
            {
                var result = await RunCommandAsync("bluetoothctl", $"disconnect {deviceAddress}");
                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from device {Address}", deviceAddress);
                return false;
            }
        }

        private List<BLEDevice> ParseDevices(string output)
        {
            var devices = new List<BLEDevice>();
            
            try
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("Device"))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var address = parts[1];
                            var name = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : "Unknown";
                            
                            var device = new BLEDevice
                            {
                                Address = address,
                                Name = name,
                                RSSI = -100, // Default RSSI
                                Type = "BLE",
                                IsMatterDevice = IsMatterDevice(name)
                            };
                            
                            devices.Add(device);
                            DeviceDiscovered?.Invoke(device);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing devices output");
            }

            return devices;
        }

        private BLEDevice ParseDeviceInfo(string address, string output)
        {
            var device = new BLEDevice
            {
                Address = address,
                Name = "Unknown",
                RSSI = -100,
                Type = "BLE"
            };

            try
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains(':'))
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();

                            switch (key.ToLower())
                            {
                                case "name":
                                    device.Name = value;
                                    break;
                                case "rssi":
                                    if (int.TryParse(value, out var rssi))
                                        device.RSSI = rssi;
                                    break;
                                case "connected":
                                    device.IsConnected = value.ToLower() == "yes";
                                    break;
                            }
                        }
                    }
                }

                device.IsMatterDevice = IsMatterDevice(device.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing device info");
            }

            return device;
        }

        private bool IsMatterDevice(string deviceName)
        {
            // Check if device name contains Matter-related keywords
            var matterKeywords = new[] { "matter", "nous", "a8m", "smart", "socket", "bulb", "switch" };
            return matterKeywords.Any(keyword => 
                deviceName.ToLower().Contains(keyword.ToLower()));
        }

        private async Task<CommandResult> RunCommandAsync(string command, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                return new CommandResult
                {
                    Success = process.ExitCode == 0,
                    Output = output,
                    Error = error,
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception ex)
            {
                return new CommandResult
                {
                    Success = false,
                    Output = string.Empty,
                    Error = ex.Message,
                    ExitCode = -1
                };
            }
        }

        private class CommandResult
        {
            public bool Success { get; set; }
            public string Output { get; set; } = string.Empty;
            public string Error { get; set; } = string.Empty;
            public int ExitCode { get; set; }
        }
    }
}
