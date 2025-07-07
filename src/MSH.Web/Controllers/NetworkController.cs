using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace MSH.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NetworkController : ControllerBase
{
    private readonly ILogger<NetworkController> _logger;
    private readonly string _networkConfigScript;

    public NetworkController(ILogger<NetworkController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _networkConfigScript = configuration["Network:ConfigScript"] ?? "/app/network-config.sh";
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        _logger.LogInformation("Ping endpoint called");
        return Ok("pong");
    }

    [HttpGet("mode")]
    public IActionResult GetCurrentMode()
    {
        try
        {
            // Check for the new commissioning mode flags
            if (System.IO.File.Exists("/etc/msh/auto_commissioning"))
            {
                return Ok("auto-commissioning");
            }
            else if (System.IO.File.Exists("/etc/msh/client_commissioning"))
            {
                return Ok("commissioning-client");
            }
            else if (System.IO.File.Exists("/etc/msh/commissioning"))
            {
                return Ok("commissioning-ap");
            }
            else
            {
                return Ok("normal");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current network mode");
            return StatusCode(500, "Error getting current network mode");
        }
    }

    [HttpPost("switch/{mode}")]
    public IActionResult SwitchMode(string mode)
    {
        // Validate the new supported modes
        var validModes = new[] { "normal", "auto-commissioning", "commissioning-client", "commissioning-ap", "commissioning", "complete" };
        
        if (!validModes.Contains(mode))
        {
            return BadRequest($"Invalid mode. Must be one of: {string.Join(", ", validModes)}");
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _networkConfigScript,
                Arguments = mode,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start network configuration script");
                return StatusCode(500, "Failed to start network configuration script");
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Network configuration script failed: {Error}", error);
                return StatusCode(500, $"Network configuration failed: {error}");
            }

            _logger.LogInformation("Successfully switched to {Mode} mode", mode);
            return Ok($"Switched to {mode} mode");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching network mode");
            return StatusCode(500, "Error switching network mode");
        }
    }

    [HttpGet("test")]
    public IActionResult TestNetworkConfig()
    {
        try
        {
            var currentMode = GetCurrentModeString();
            var result = new
            {
                ScriptExists = System.IO.File.Exists(_networkConfigScript),
                ScriptPath = _networkConfigScript,
                CurrentMode = currentMode,
                ScriptPermissions = GetScriptPermissions(),
                NetworkInterfaces = GetNetworkInterfaces(),
                GuiAccessible = IsGuiAccessible(currentMode)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing network configuration");
            return StatusCode(500, $"Error testing network configuration: {ex.Message}");
        }
    }

    [HttpGet("status")]
    public IActionResult GetDetailedStatus()
    {
        try
        {
            var currentMode = GetCurrentModeString();
            var status = new
            {
                CurrentMode = currentMode,
                GuiAccessible = IsGuiAccessible(currentMode),
                NetworkInfo = GetNetworkInfo(currentMode),
                AvailableModes = new[]
                {
                    new { Mode = "normal", Description = "Normal client mode", GuiAccessible = true },
                    new { Mode = "auto-commissioning", Description = "GUI-driven commissioning workflow", GuiAccessible = true },
                    new { Mode = "commissioning-client", Description = "Safe client commissioning mode", GuiAccessible = true },
                    new { Mode = "commissioning-ap", Description = "AP mode for device control", GuiAccessible = false },
                    new { Mode = "complete", Description = "Complete commissioning and return to client", GuiAccessible = true }
                }
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed status");
            return StatusCode(500, $"Error getting detailed status: {ex.Message}");
        }
    }

    private string GetCurrentModeString()
    {
        if (System.IO.File.Exists("/etc/msh/auto_commissioning"))
        {
            return "auto-commissioning";
        }
        else if (System.IO.File.Exists("/etc/msh/client_commissioning"))
        {
            return "commissioning-client";
        }
        else if (System.IO.File.Exists("/etc/msh/commissioning"))
        {
            return "commissioning-ap";
        }
        else
        {
            return "normal";
        }
    }

    private bool IsGuiAccessible(string mode)
    {
        return mode switch
        {
            "normal" => true,
            "auto-commissioning" => true,
            "commissioning-client" => true,
            "commissioning-ap" => false,
            _ => true
        };
    }

    private object GetNetworkInfo(string mode)
    {
        return mode switch
        {
            "normal" => new { 
                Network = "Main network", 
                IP = "192.168.0.104", 
                Access = "Full access" 
            },
            "auto-commissioning" => new { 
                Network = "Main network", 
                IP = "192.168.0.104", 
                Access = "GUI accessible, BLE commissioning active" 
            },
            "commissioning-client" => new { 
                Network = "Main network", 
                IP = "192.168.0.104", 
                Access = "GUI accessible, safe for BLE commissioning" 
            },
            "commissioning-ap" => new { 
                Network = "AP network", 
                IP = "192.168.4.1", 
                Access = "GUI temporarily unavailable" 
            },
            _ => new { 
                Network = "Unknown", 
                IP = "Unknown", 
                Access = "Unknown" 
            }
        };
    }

    private string GetScriptPermissions()
    {
        try
        {
            var fileInfo = new FileInfo(_networkConfigScript);
            return fileInfo.Exists ? fileInfo.UnixFileMode.ToString() : "File not found";
        }
        catch
        {
            return "Unable to get permissions";
        }
    }

    private object GetNetworkInterfaces()
    {
        try
        {
            var interfaces = new List<object>();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ip",
                    Arguments = "addr show",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return new { RawOutput = output };
        }
        catch
        {
            return new { Error = "Unable to get network interfaces" };
        }
    }
} 