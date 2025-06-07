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
            if (System.IO.File.Exists("/etc/msh/commissioning"))
            {
                return Ok("commissioning");
            }
            return Ok("normal");
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
        if (mode != "normal" && mode != "commissioning")
        {
            return BadRequest("Invalid mode. Must be 'normal' or 'commissioning'");
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
            var result = new
            {
                ScriptExists = System.IO.File.Exists(_networkConfigScript),
                ScriptPath = _networkConfigScript,
                CurrentMode = System.IO.File.Exists("/etc/msh/commissioning") ? "commissioning" : "normal",
                ScriptPermissions = GetScriptPermissions(),
                NetworkInterfaces = GetNetworkInterfaces()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing network configuration");
            return StatusCode(500, $"Error testing network configuration: {ex.Message}");
        }
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