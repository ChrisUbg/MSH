using Microsoft.AspNetCore.Mvc;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Interfaces;
using System.Text.Json;

namespace MSH.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceActionDelayController : ControllerBase
{
    private readonly IDeviceActionDelayService _deviceActionDelayService;
    private readonly ILogger<DeviceActionDelayController> _logger;

    public DeviceActionDelayController(IDeviceActionDelayService deviceActionDelayService, ILogger<DeviceActionDelayController> logger)
    {
        _deviceActionDelayService = deviceActionDelayService;
        _logger = logger;
    }

    // GET: api/deviceactiondelay
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceActionDelay>>> GetActionDelays()
    {
        try
        {
            var pendingActions = await _deviceActionDelayService.GetPendingActionDelaysAsync();
            return Ok(pendingActions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action delays");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/deviceactiondelay/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<DeviceActionDelay>> GetActionDelay(Guid id)
    {
        try
        {
            var actionDelay = await _deviceActionDelayService.GetActionDelayByIdAsync(id);
            if (actionDelay == null)
            {
                return NotFound();
            }
            return Ok(actionDelay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action delay {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/deviceactiondelay/device/{deviceId}
    [HttpGet("device/{deviceId}")]
    public async Task<ActionResult<IEnumerable<DeviceActionDelay>>> GetActionDelaysForDevice(Guid deviceId)
    {
        try
        {
            var actionDelays = await _deviceActionDelayService.GetActionDelaysForDeviceAsync(deviceId);
            return Ok(actionDelays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action delays for device {DeviceId}", deviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/deviceactiondelay/group/{groupId}
    [HttpGet("group/{groupId}")]
    public async Task<ActionResult<IEnumerable<DeviceActionDelay>>> GetActionDelaysForGroup(Guid groupId)
    {
        try
        {
            var actionDelays = await _deviceActionDelayService.GetActionDelaysForDeviceGroupAsync(groupId);
            return Ok(actionDelays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action delays for group {GroupId}", groupId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/deviceactiondelay
    [HttpPost]
    public async Task<ActionResult<DeviceActionDelay>> CreateActionDelay([FromBody] DeviceActionDelay actionDelay)
    {
        try
        {
            var createdActionDelay = await _deviceActionDelayService.CreateActionDelayAsync(actionDelay);
            return CreatedAtAction(nameof(GetActionDelay), new { id = createdActionDelay.Id }, createdActionDelay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating action delay");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/deviceactiondelay/schedule/device
    [HttpPost("schedule/device")]
    public async Task<ActionResult<bool>> ScheduleDeviceAction([FromBody] ScheduleDeviceActionRequest request)
    {
        try
        {
            var success = await _deviceActionDelayService.ScheduleDeviceActionAsync(
                request.DeviceId, 
                request.ActionType, 
                request.DelaySeconds, 
                request.Parameters);
            
            if (success)
            {
                return Ok(new { message = "Action scheduled successfully" });
            }
            return BadRequest("Failed to schedule action");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling device action");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/deviceactiondelay/schedule/group
    [HttpPost("schedule/group")]
    public async Task<ActionResult<bool>> ScheduleGroupAction([FromBody] ScheduleGroupActionRequest request)
    {
        try
        {
            var success = await _deviceActionDelayService.ScheduleDeviceGroupActionAsync(
                request.DeviceGroupId, 
                request.ActionType, 
                request.DelaySeconds, 
                request.Parameters);
            
            if (success)
            {
                return Ok(new { message = "Group action scheduled successfully" });
            }
            return BadRequest("Failed to schedule group action");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling group action");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/deviceactiondelay/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<DeviceActionDelay>> UpdateActionDelay(Guid id, [FromBody] DeviceActionDelay actionDelay)
    {
        try
        {
            if (id != actionDelay.Id)
            {
                return BadRequest();
            }

            var updatedActionDelay = await _deviceActionDelayService.UpdateActionDelayAsync(actionDelay);
            return Ok(updatedActionDelay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating action delay {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/deviceactiondelay/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteActionDelay(Guid id)
    {
        try
        {
            var success = await _deviceActionDelayService.DeleteActionDelayAsync(id);
            if (success)
            {
                return Ok(new { message = "Action delay deleted successfully" });
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting action delay {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/deviceactiondelay/{id}/execute
    [HttpPost("{id}/execute")]
    public async Task<ActionResult<bool>> ExecuteActionDelay(Guid id)
    {
        try
        {
            var success = await _deviceActionDelayService.ExecuteActionDelayAsync(id);
            if (success)
            {
                return Ok(new { message = "Action executed successfully" });
            }
            return BadRequest("Failed to execute action");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action delay {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/deviceactiondelay/execute-all
    [HttpPost("execute-all")]
    public async Task<ActionResult<bool>> ExecuteAllPendingActions()
    {
        try
        {
            var success = await _deviceActionDelayService.ExecuteAllPendingActionDelaysAsync();
            return Ok(new { message = "All pending actions executed", success });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing all pending actions");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/deviceactiondelay/{id}/cancel
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<bool>> CancelActionDelay(Guid id)
    {
        try
        {
            var success = await _deviceActionDelayService.CancelPendingActionAsync(id);
            if (success)
            {
                return Ok(new { message = "Action cancelled successfully" });
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling action delay {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/deviceactiondelay/{id}/status
    [HttpGet("{id}/status")]
    public async Task<ActionResult<Dictionary<string, object>>> GetActionDelayStatus(Guid id)
    {
        try
        {
            var status = await _deviceActionDelayService.GetActionDelayStatusAsync(id);
            if (status.Any())
            {
                return Ok(status);
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action delay status {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

// Request models
public class ScheduleDeviceActionRequest
{
    public Guid DeviceId { get; set; }
    public string ActionType { get; set; } = null!;
    public int DelaySeconds { get; set; }
    public string? Parameters { get; set; }
}

public class ScheduleGroupActionRequest
{
    public Guid DeviceGroupId { get; set; }
    public string ActionType { get; set; } = null!;
    public int DelaySeconds { get; set; }
    public string? Parameters { get; set; }
}
