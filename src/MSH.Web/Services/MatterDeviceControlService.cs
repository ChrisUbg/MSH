using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MSH.Web.Services
{
    public class MatterDeviceControlService : IMatterDeviceControlService
    {
        private readonly ILogger<MatterDeviceControlService> _logger;

        public MatterDeviceControlService(ILogger<MatterDeviceControlService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ToggleDeviceAsync(string nodeId)
        {
            try
            {
                _logger.LogInformation("Toggling device {NodeId}", nodeId);
                var result = await ExecuteChipToolCommand($"onoff toggle {nodeId} 1");
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
                var result = await ExecuteChipToolCommand($"onoff on {nodeId} 1");
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
                var result = await ExecuteChipToolCommand($"onoff off {nodeId} 1");
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
                _logger.LogInformation("Getting state for device {NodeId}", nodeId);
                var result = await ExecuteChipToolCommandWithOutput($"onoff read on-off {nodeId} 1");
                
                // Parse the output to extract the state
                if (result.Contains("on"))
                    return "on";
                else if (result.Contains("off"))
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
                _logger.LogInformation("Checking if device {NodeId} is online", nodeId);
                var result = await ExecuteChipToolCommandWithOutput($"onoff read on-off {nodeId} 1");
                return !result.Contains("Timeout") && !result.Contains("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if device {NodeId} is online", nodeId);
                return false;
            }
        }

        private async Task<bool> ExecuteChipToolCommand(string command)
        {
            // Convert hex Node ID to proper format for chip-tool
            var formattedCommand = FormatCommandForChipTool(command);
            var dockerCommand = $"docker exec chip_tool /usr/sbin/chip-tool {formattedCommand}";
            
            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{dockerCommand}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            _logger.LogInformation("Executing: {Command}", dockerCommand);

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Command failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return false;
            }

            _logger.LogInformation("Command output: {Output}", output);
            return true;
        }

        private async Task<string> ExecuteChipToolCommandWithOutput(string command)
        {
            // Convert hex Node ID to proper format for chip-tool
            var formattedCommand = FormatCommandForChipTool(command);
            var dockerCommand = $"docker exec chip_tool /usr/sbin/chip-tool {formattedCommand}";
            
            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{dockerCommand}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            _logger.LogInformation("Executing: {Command}", dockerCommand);

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Command failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return $"Error: {error}";
            }

            _logger.LogInformation("Command output: {Output}", output);
            return output;
        }

        private string FormatCommandForChipTool(string command)
        {
            // Split command into parts
            var parts = command.Split(' ');
            if (parts.Length >= 4)
            {
                // Check if the third part (index 2) looks like a hex Node ID
                var nodeIdPart = parts[2];
                if (nodeIdPart.Length == 16 && IsHexString(nodeIdPart))
                {
                    // Convert to proper hex format with 0x prefix
                    parts[2] = $"0x{nodeIdPart}";
                    return string.Join(" ", parts);
                }
            }
            return command;
        }

        private bool IsHexString(string str)
        {
            return str.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
        }


    }
}
