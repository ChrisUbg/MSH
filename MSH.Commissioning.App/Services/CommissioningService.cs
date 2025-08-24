using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MSH.Commissioning.App.Models;

namespace MSH.Commissioning.App.Services
{
    public class CommissioningService : ICommissioningService
    {
        private readonly ILogger<CommissioningService> _logger;
        private readonly IBLEScannerService _bleScanner;

        public event Action<CommissioningProgress>? ProgressUpdated;

        public CommissioningService(ILogger<CommissioningService> logger, IBLEScannerService bleScanner)
        {
            _logger = logger;
            _bleScanner = bleScanner;
        }

        public async Task<CommissioningResult> StartCommissioningAsync(CommissioningRequest request, string sessionId)
        {
            try
            {
                _logger.LogInformation("Starting commissioning for device: {DeviceName}", request.DeviceName);
                
                // Update progress
                UpdateProgress(sessionId, 10, "Initializing commissioning process...");

                // Step 1: Check if chip-tool is available
                if (!await IsChipToolAvailableAsync())
                {
                    return new CommissioningResult
                    {
                        Success = false,
                        Message = "chip-tool not found. Please install Matter SDK.",
                        ErrorDetails = "chip-tool command not available"
                    };
                }

                UpdateProgress(sessionId, 20, "chip-tool found, preparing commissioning...");

                // Step 2: Generate Node ID if not provided
                if (string.IsNullOrEmpty(request.NodeId))
                {
                    request.NodeId = GenerateNodeId();
                    _logger.LogInformation("Generated Node ID: {NodeId}", request.NodeId);
                }

                UpdateProgress(sessionId, 30, "Node ID prepared, starting BLE commissioning...");

                // Step 3: Execute commissioning command
                var commissioningResult = await ExecuteCommissioningCommandAsync(request);
                
                if (!commissioningResult.Success)
                {
                    UpdateProgress(sessionId, 100, "Commissioning failed", true, commissioningResult.ErrorDetails);
                    return commissioningResult;
                }

                UpdateProgress(sessionId, 80, "Commissioning successful, testing device...");

                // Step 4: Test device connection
                var testResult = await TestDeviceConnectionAsync(request.NodeId);
                if (!testResult.Success)
                {
                    _logger.LogWarning("Device commissioning succeeded but connection test failed");
                }

                UpdateProgress(sessionId, 90, "Device tested, preparing transfer to Pi...");

                // Step 5: Transfer to Pi
                var transferResult = await TransferToPiAsync(request.NodeId, request.PiIP, request.PiUser);
                if (!transferResult)
                {
                    _logger.LogWarning("Failed to transfer device to Pi, but commissioning was successful");
                }

                UpdateProgress(sessionId, 100, "Commissioning completed successfully!");

                return new CommissioningResult
                {
                    Success = true,
                    Message = "Device commissioned successfully",
                    NodeId = request.NodeId,
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during commissioning");
                UpdateProgress(sessionId, 100, "Commissioning failed", true, ex.Message);
                
                return new CommissioningResult
                {
                    Success = false,
                    Message = "Commissioning failed",
                    ErrorDetails = ex.Message,
                    Timestamp = DateTime.Now
                };
            }
        }

        public async Task<CommissioningResult> TestDeviceConnectionAsync(string deviceAddress)
        {
            try
            {
                _logger.LogInformation("Testing device connection: {Address}", deviceAddress);

                // Test device read command
                var result = await RunChipToolCommandAsync($"onoff read on-off {deviceAddress} 1");
                
                return new CommissioningResult
                {
                    Success = result.Success,
                    Message = result.Success ? "Device connection test successful" : "Device connection test failed",
                    ErrorDetails = result.Success ? null : result.Error,
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing device connection");
                return new CommissioningResult
                {
                    Success = false,
                    Message = "Device connection test failed",
                    ErrorDetails = ex.Message,
                    Timestamp = DateTime.Now
                };
            }
        }

        public async Task<bool> TransferToPiAsync(string deviceId, string piIp, string piUser)
        {
            try
            {
                _logger.LogInformation("Transferring device {DeviceId} to Pi {PiIp}", deviceId, piIp);

                // This would involve transferring the KVS file and device credentials
                // For now, we'll just test the SSH connection
                var result = await RunCommandAsync("ssh", $"{piUser}@{piIp} 'echo Device transfer test successful'");
                
                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring device to Pi");
                return false;
            }
        }

        private async Task<bool> IsChipToolAvailableAsync()
        {
            try
            {
                var result = await RunCommandAsync("chip-tool", "--version");
                return result.Success;
            }
            catch
            {
                return false;
            }
        }

        private async Task<CommissioningResult> ExecuteCommissioningCommandAsync(CommissioningRequest request)
        {
            try
            {
                // Build the commissioning command based on the documented process
                var command = $"pairing ble-wifi {request.Discriminator} {request.Passcode} {request.NetworkSSID} {request.NetworkPassword} --bypass-attestation-verifier true --node-id 0x{request.NodeId}";
                
                _logger.LogInformation("Executing commissioning command: chip-tool {Command}", command);
                
                var result = await RunChipToolCommandAsync(command);
                
                return new CommissioningResult
                {
                    Success = result.Success,
                    Message = result.Success ? "Commissioning command executed successfully" : "Commissioning command failed",
                    ErrorDetails = result.Success ? null : result.Error,
                    NodeId = request.NodeId,
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing commissioning command");
                return new CommissioningResult
                {
                    Success = false,
                    Message = "Commissioning command failed",
                    ErrorDetails = ex.Message,
                    Timestamp = DateTime.Now
                };
            }
        }

        private string GenerateNodeId()
        {
            // Generate a unique 64-bit Node ID
            var random = new Random();
            var nodeId = (ulong)random.NextInt64((long)0x1000000000000000, long.MaxValue);
            return nodeId.ToString("X16");
        }

        private async Task<CommandResult> RunChipToolCommandAsync(string arguments)
        {
            return await RunCommandAsync("chip-tool", arguments);
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

        private void UpdateProgress(string sessionId, int percentage, string message, bool hasError = false, string? errorMessage = null)
        {
            var progress = new CommissioningProgress
            {
                Percentage = percentage,
                Message = message,
                IsComplete = percentage >= 100,
                HasError = hasError,
                ErrorMessage = errorMessage
            };

            ProgressUpdated?.Invoke(progress);
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
