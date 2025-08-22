using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace MSH.Web.Services
{
    public class MatterDeviceControlService : IMatterDeviceControlService
    {
        private readonly ILogger<MatterDeviceControlService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _matterApiBaseUrl;
        private readonly IDeviceEventService? _eventService;

        public MatterDeviceControlService(ILogger<MatterDeviceControlService> logger, HttpClient httpClient, IConfiguration configuration, IDeviceEventService? eventService = null)
        {
            _logger = logger;
            _httpClient = httpClient;
            _eventService = eventService;
            
            // Get API URL from configuration, with fallback
            _matterApiBaseUrl = configuration["Matter:ApiBaseUrl"] ?? "http://localhost:8000/api/matter";
            
            _logger.LogInformation("MatterDeviceControlService initialized with API URL: {ApiUrl}", _matterApiBaseUrl);
        }

        public async Task<bool> ToggleDeviceAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Toggling device {NodeId} via HTTP API", nodeId);
                
                // Get current state before toggle for event logging
                var currentState = await GetDeviceStateAsync(nodeId);
                
                // NEW: Try HTTP API first (fast)
                try
                {
                    var response = await _httpClient.PostAsync($"{_matterApiBaseUrl}/device/{nodeId}/toggle", null);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("HTTP API toggle successful for device {NodeId}", nodeId);
                        
                        // Log the toggle event
                        await LogDeviceEventAsync(nodeId, "Power Toggle", currentState, "toggled", "Device power toggled via HTTP API", "HTTP API");
                        
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("HTTP API toggle failed for device {NodeId}, status: {StatusCode}", nodeId, response.StatusCode);
                    }
                }
                catch (Exception httpEx)
                {
                    _logger.LogWarning(httpEx, "HTTP API toggle failed for device {NodeId}, falling back to Docker", nodeId);
                }

                // FALLBACK: Use Docker command (slow but reliable)
                _logger.LogInformation("Falling back to Docker command for device {NodeId}", nodeId);
                // Fix: Add 0x prefix to node ID for chip-tool
                var formattedNodeId = nodeId.StartsWith("0x") ? nodeId : $"0x{nodeId}";
                var result = await ExecuteChipToolCommand($"onoff toggle {formattedNodeId} 1");
                
                if (result)
                {
                    // Log the toggle event
                    await LogDeviceEventAsync(nodeId, "Power Toggle", currentState, "toggled", "Device power toggled via Docker fallback", "Docker");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling device {NodeId}", nodeId);
                return false;
            }
        }

        public async Task<bool> TurnOnDeviceAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Turning on device {NodeId}", nodeId);
                // Fix: Add 0x prefix to node ID for chip-tool
                var formattedNodeId = nodeId.StartsWith("0x") ? nodeId : $"0x{nodeId}";
                var result = await ExecuteChipToolCommand($"onoff on {formattedNodeId} 1");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error turning on device {NodeId}", nodeId);
                return false;
            }
        }

        public async Task<bool> TurnOffDeviceAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Turning off device {NodeId}", nodeId);
                // Fix: Add 0x prefix to node ID for chip-tool
                var formattedNodeId = nodeId.StartsWith("0x") ? nodeId : $"0x{nodeId}";
                var result = await ExecuteChipToolCommand($"onoff off {formattedNodeId} 1");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error turning off device {NodeId}", nodeId);
                return false;
            }
        }

        public async Task<string?> GetDeviceStateAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Getting state for device {NodeId} via HTTP API", nodeId);
                
                // NEW: Try HTTP API first (fast)
                try
                {
                    var response = await _httpClient.GetAsync($"{_matterApiBaseUrl}/device/{nodeId}/state");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var stateResponse = JsonSerializer.Deserialize<DeviceStateResponse>(jsonResponse);
                        
                        if (stateResponse?.State != null)
                        {
                            _logger.LogInformation("HTTP API state successful for device {NodeId}: {State}", nodeId, stateResponse.State);
                            return stateResponse.State;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("HTTP API state failed for device {NodeId}, status: {StatusCode}", nodeId, response.StatusCode);
                    }
                }
                catch (Exception httpEx)
                {
                    _logger.LogWarning(httpEx, "HTTP API state failed for device {NodeId}, falling back to Docker", nodeId);
                }

                // FALLBACK: Use Docker command (slow but reliable)
                _logger.LogInformation("Falling back to Docker command for device {NodeId}", nodeId);
                // Fix: Add 0x prefix to node ID for chip-tool
                var formattedNodeId = nodeId.StartsWith("0x") ? nodeId : $"0x{nodeId}";
                var result = await ExecuteChipToolCommandWithOutput($"onoff read on-off {formattedNodeId} 1");
                
                // Parse the output to extract the state
                if (result.Contains("OnOff: TRUE"))
                    return "on";
                else if (result.Contains("OnOff: FALSE"))
                    return "off";
                else
                    return "unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting state for device {NodeId}", nodeId);
                return "unknown";
            }
        }

        public async Task<bool> IsDeviceOnlineAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Checking if device {NodeId} is online via HTTP API", nodeId);
                
                // NEW: Try HTTP API first (fast)
                try
                {
                    var response = await _httpClient.GetAsync($"{_matterApiBaseUrl}/device/{nodeId}/online");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var onlineResponse = JsonSerializer.Deserialize<DeviceOnlineResponse>(jsonResponse);
                        
                        if (onlineResponse != null)
                        {
                            _logger.LogInformation("HTTP API online check successful for device {NodeId}: {IsOnline}", nodeId, onlineResponse.IsOnline);
                            return onlineResponse.IsOnline;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("HTTP API online check failed for device {NodeId}, status: {StatusCode}", nodeId, response.StatusCode);
                    }
                }
                catch (Exception httpEx)
                {
                    _logger.LogWarning(httpEx, "HTTP API online check failed for device {NodeId}, falling back to Docker", nodeId);
                }

                // FALLBACK: Use Docker command (slow but reliable)
                _logger.LogInformation("Falling back to Docker command for device {NodeId}", nodeId);
                // Fix: Add 0x prefix to node ID for chip-tool
                var formattedNodeId = nodeId.StartsWith("0x") ? nodeId : $"0x{nodeId}";
                var result = await ExecuteChipToolCommandWithOutput($"onoff read on-off {formattedNodeId} 1");
                return !result.Contains("Timeout") && !result.Contains("Error") && !result.Contains("Invalid argument");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if device {NodeId} is online", nodeId);
                return false;
            }
        }

        public async Task<PowerMetricsResult?> GetPowerMetricsAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Getting power metrics for device {NodeId} via HTTP API", nodeId);
                
                // NEW: Try HTTP API first (fast)
                try
                {
                    var response = await _httpClient.GetAsync($"{_matterApiBaseUrl}/device/{nodeId}/power-metrics");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var metricsResponse = JsonSerializer.Deserialize<PowerMetricsResponse>(jsonResponse);
                        
                        if (metricsResponse != null)
                        {
                            _logger.LogInformation("HTTP API power metrics successful for device {NodeId}", nodeId);
                            return new PowerMetricsResult
                            {
                                DeviceId = metricsResponse.DeviceId,
                                PowerState = metricsResponse.PowerState,
                                PowerConsumption = metricsResponse.PowerConsumption,
                                Voltage = metricsResponse.Voltage,
                                Current = metricsResponse.Current,
                                EnergyToday = metricsResponse.EnergyToday,
                                Online = metricsResponse.Online,
                                LastUpdated = DateTime.UtcNow
                            };
                        }
                    }
                    else
                    {
                        _logger.LogWarning("HTTP API power metrics failed for device {NodeId}, status: {StatusCode}", nodeId, response.StatusCode);
                    }
                }
                catch (Exception httpEx)
                {
                    _logger.LogWarning(httpEx, "HTTP API power metrics failed for device {NodeId}, falling back to Docker", nodeId);
                }

                // FALLBACK: Use Docker electrical measurement commands (slow but reliable)
                _logger.LogInformation("Falling back to Docker electrical measurement for device {NodeId}", nodeId);
                
                try
                {
                    // Get device state first
                    var state = await GetDeviceStateAsync(nodeId);
                    
                    // Fix: Add 0x prefix to node ID for chip-tool
                    var formattedNodeId = nodeId.StartsWith("0x") ? nodeId : $"0x{nodeId}";
                    
                    // Try to get power measurement using electrical measurement cluster
                    // Note: This may fail for devices that don't support electrical measurement
                    decimal? power = null;
                    decimal? energy = null;
                    
                    try
                    {
                        var powerResult = await ExecuteChipToolCommandWithOutput($"electricalmeasurement read measurement-type {formattedNodeId} 1");
                        
                        // Parse power measurement
                        foreach (var line in powerResult.Split('\n'))
                        {
                            if (line.Contains("measurement-type:"))
                            {
                                var value = line.Split(':')[1].Trim();
                                if (decimal.TryParse(value, out var powerValue))
                                {
                                    power = powerValue;
                                }
                            }
                        }
                    }
                    catch (Exception powerEx)
                    {
                        _logger.LogDebug("Power measurement not supported for device {NodeId}: {Error}", nodeId, powerEx.Message);
                    }
                    
                    try
                    {
                        var energyResult = await ExecuteChipToolCommandWithOutput($"electricalmeasurement read total-energy {formattedNodeId} 1");
                        
                        // Parse energy measurement
                        foreach (var line in energyResult.Split('\n'))
                        {
                            if (line.Contains("total-energy:"))
                            {
                                var value = line.Split(':')[1].Trim();
                                if (decimal.TryParse(value, out var energyValue))
                                {
                                    energy = energyValue;
                                }
                            }
                        }
                    }
                    catch (Exception energyEx)
                    {
                        _logger.LogDebug("Energy measurement not supported for device {NodeId}: {Error}", nodeId, energyEx.Message);
                    }
                    
                    return new PowerMetricsResult
                    {
                        DeviceId = nodeId,
                        PowerState = state,
                        PowerConsumption = power ?? 0,
                        Voltage = 230.0m, // Default voltage estimate
                        Current = power.HasValue && power > 0 ? power / 230.0m : 0,
                        EnergyToday = energy,
                        Online = !string.IsNullOrEmpty(state) && state != "unknown" && state != "offline",
                        LastUpdated = DateTime.UtcNow
                    };
                }
                catch (Exception dockerEx)
                {
                    _logger.LogWarning(dockerEx, "Docker electrical measurement failed for device {NodeId}", nodeId);
                    
                    // Return basic power metrics with state-based estimates
                    var basicState = await GetDeviceStateAsync(nodeId);
                    var isOn = basicState == "on";
                    
                    return new PowerMetricsResult
                    {
                        DeviceId = nodeId,
                        PowerState = basicState,
                        PowerConsumption = isOn ? 50.0m : 0, // Estimate 50W when on
                        Voltage = 230.0m, // Standard voltage
                        Current = isOn ? 0.217m : 0, // 50W / 230V
                        EnergyToday = isOn ? 0.1m : 0, // Small energy value
                        Online = !string.IsNullOrEmpty(basicState) && basicState != "unknown" && basicState != "offline",
                        LastUpdated = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting power metrics for device {NodeId}", nodeId);
                return null;
            }
        }

        private async Task<bool> ExecuteChipToolCommand(string command)
        {
            _logger.LogInformation("ExecuteChipToolCommand called with command: '{Command}'", command);
            
            // Use the fast command wrapper for better performance
            var formattedCommand = FormatCommandForChipTool(command);
            var fastCommand = $"/usr/local/bin/fast-chip-tool.sh {formattedCommand}";
            
            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{fastCommand}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            _logger.LogInformation("Executing fast command: {Command}", fastCommand);

            process.Start();
            
            // Increased timeout to 15 seconds for the fast wrapper
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15)); // 15 second timeout
            var processTask = Task.Run(async () =>
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                return (output, error);
            });

            var completedTask = await Task.WhenAny(processTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                _logger.LogWarning("Fast command timed out after 15 seconds: {Command}", fastCommand);
                try { process.Kill(); } catch { }
                return false;
            }

            var (output, error) = await processTask;

            _logger.LogInformation("Fast command completed. Exit code: {ExitCode}, Output: {Output}, Error: {Error}", process.ExitCode, output, error);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Fast command failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return false;
            }

            _logger.LogInformation("Fast command succeeded with output: {Output}", output);
            return true;
        }

        private async Task<string> ExecuteChipToolCommandWithOutput(string command)
        {
            // Use the fast command wrapper for better performance
            var formattedCommand = FormatCommandForChipTool(command);
            var fastCommand = $"/usr/local/bin/fast-chip-tool.sh {formattedCommand}";
            
            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{fastCommand}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            _logger.LogInformation("Executing fast command with output: {Command}", fastCommand);

            process.Start();
            
            // Increased timeout to 15 seconds for the fast wrapper
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15)); // 15 second timeout
            var processTask = Task.Run(async () =>
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                return (output, error);
            });

            var completedTask = await Task.WhenAny(processTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                _logger.LogWarning("Fast command timed out after 15 seconds: {Command}", fastCommand);
                try { process.Kill(); } catch { }
                return "Error: Command timed out";
            }

            var (output, error) = await processTask;

            _logger.LogInformation("Fast command output: {Output}", output);
            _logger.LogInformation("Fast command error: {Error}", error);
            _logger.LogInformation("Fast command exit code: {ExitCode}", process.ExitCode);
            
            // Check if the output contains the expected result, regardless of exit code
            if (output.Contains("OnOff: TRUE") || output.Contains("OnOff: FALSE"))
            {
                _logger.LogInformation("Fast command succeeded with valid output");
                return output;
            }
            
            // For electricalmeasurement commands, check if we got any output (even if it's an error about unsupported attributes)
            if (command.Contains("electricalmeasurement"))
            {
                if (!string.IsNullOrEmpty(output) || !string.IsNullOrEmpty(error))
                {
                    _logger.LogInformation("Electrical measurement command completed (may not be supported by device)");
                    return output;
                }
            }
            
            if (process.ExitCode != 0)
            {
                _logger.LogError("Fast command failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return $"Error: {error}";
            }

            return output;
        }

        private string FormatCommandForChipTool(string command)
        {
            // Manual 0x prefix addition is now handled in the calling methods
            // This method is kept for compatibility but doesn't modify the command
            return command;
        }

        private bool IsHexString(string str)
        {
            return str.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
        }

        private async Task LogDeviceEventAsync(string nodeId, string eventType, string? oldValue, string? newValue, string? description, string? source)
        {
            try
            {
                if (_eventService != null)
                {
                    // Try to find the device ID from the node ID
                    // For now, we'll use a simple approach - in a real system, you'd have a mapping
                    var deviceId = Guid.NewGuid(); // TODO: Get actual device ID from node ID mapping
                    
                    await _eventService.LogDeviceEventAsync(deviceId, eventType, oldValue, newValue, description, source);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log device event for node {NodeId}", nodeId);
            }
        }

        // Response models for HTTP API
        private class DeviceStateResponse
        {
            public string? State { get; set; }
            public bool Success { get; set; }
            public string? Message { get; set; }
        }

        private class DeviceToggleResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public string? NewState { get; set; }
        }

        private class DeviceOnlineResponse
        {
            public bool IsOnline { get; set; }
            public bool Success { get; set; }
            public string? Message { get; set; }
        }

        private class PowerMetricsResponse
        {
            public string? DeviceId { get; set; }
            public string? PowerState { get; set; }
            public decimal? PowerConsumption { get; set; }
            public decimal? Voltage { get; set; }
            public decimal? Current { get; set; }
            public decimal? EnergyToday { get; set; }
            public bool Online { get; set; }
            public bool Success { get; set; }
            public string? Message { get; set; }
        }
    }
}
