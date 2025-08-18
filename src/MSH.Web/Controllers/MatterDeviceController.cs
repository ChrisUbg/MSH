using Microsoft.AspNetCore.Mvc;
using MSH.Web.Services;
using System.Threading.Tasks;

namespace MSH.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatterDeviceController : ControllerBase
{
    private readonly IMatterDeviceControlService _matterDeviceControlService;
    private readonly ILogger<MatterDeviceController> _logger;

    public MatterDeviceController(
        IMatterDeviceControlService matterDeviceControlService,
        ILogger<MatterDeviceController> logger)
    {
        _matterDeviceControlService = matterDeviceControlService;
        _logger = logger;
    }

    [HttpPost("{nodeId}/toggle")]
    public async Task<IActionResult> ToggleDevice(string nodeId)
    {
        try
        {
            _logger.LogInformation("API: Toggling device with Node ID: {NodeId}", nodeId);
            
            var success = await _matterDeviceControlService.ToggleDeviceAsync(nodeId);
            
            if (success)
            {
                return Ok(new { success = true, message = "Device toggled successfully", nodeId });
            }
            else
            {
                return BadRequest(new { success = false, message = "Failed to toggle device", nodeId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error toggling device {NodeId}", nodeId);
            return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("{nodeId}/on")]
    public async Task<IActionResult> TurnOnDevice(string nodeId)
    {
        try
        {
            _logger.LogInformation("API: Turning ON device with Node ID: {NodeId}", nodeId);
            
            var success = await _matterDeviceControlService.TurnOnDeviceAsync(nodeId);
            
            if (success)
            {
                return Ok(new { success = true, message = "Device turned ON successfully", nodeId });
            }
            else
            {
                return BadRequest(new { success = false, message = "Failed to turn ON device", nodeId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error turning ON device {NodeId}", nodeId);
            return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("{nodeId}/off")]
    public async Task<IActionResult> TurnOffDevice(string nodeId)
    {
        try
        {
            _logger.LogInformation("API: Turning OFF device with Node ID: {NodeId}", nodeId);
            
            var success = await _matterDeviceControlService.TurnOffDeviceAsync(nodeId);
            
            if (success)
            {
                return Ok(new { success = true, message = "Device turned OFF successfully", nodeId });
            }
            else
            {
                return BadRequest(new { success = false, message = "Failed to turn OFF device", nodeId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error turning OFF device {NodeId}", nodeId);
            return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("{nodeId}/state")]
    public async Task<IActionResult> GetDeviceState(string nodeId)
    {
        try
        {
            _logger.LogInformation("API: Getting state for device with Node ID: {NodeId}", nodeId);
            
            var state = await _matterDeviceControlService.GetDeviceStateAsync(nodeId);
            
            if (!string.IsNullOrEmpty(state))
            {
                return Ok(new { success = true, state, nodeId });
            }
            else
            {
                return NotFound(new { success = false, message = "Device not found or not responding", nodeId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting state for device {NodeId}", nodeId);
            return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("{nodeId}/online")]
    public async Task<IActionResult> IsDeviceOnline(string nodeId)
    {
        try
        {
            _logger.LogInformation("API: Checking if device is online: {NodeId}", nodeId);
            
            var isOnline = await _matterDeviceControlService.IsDeviceOnlineAsync(nodeId);
            
            return Ok(new { success = true, isOnline, nodeId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error checking if device is online {NodeId}", nodeId);
            return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("commissioned")]
    public IActionResult GetCommissionedDevices()
    {
        // Return the list of known commissioned devices
        var devices = new[]
        {
            new { name = "Office Socket 1", nodeId = "4328ED19954E9DC0", type = "NOUS A8M" },
            new { name = "Office Socket 2", nodeId = "4328ED19954E9DC1", type = "NOUS A8M" }
        };

        return Ok(new { success = true, devices });
    }
}
