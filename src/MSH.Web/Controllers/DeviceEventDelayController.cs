using Microsoft.AspNetCore.Mvc;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Interfaces;

namespace MSH.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceEventDelayController : ControllerBase
{
    private readonly IDeviceEventDelayService _eventDelayService;
    private readonly ILogger<DeviceEventDelayController> _logger;

    public DeviceEventDelayController(IDeviceEventDelayService eventDelayService, ILogger<DeviceEventDelayController> logger)
    {
        _eventDelayService = eventDelayService;
        _logger = logger;
    }

    [HttpGet("device/{deviceId}")]
    public async Task<ActionResult<IEnumerable<DeviceEventDelay>>> GetEventDelaysForDevice(Guid deviceId)
    {
        try
        {
            var eventDelays = await _eventDelayService.GetEventDelaysForDeviceAsync(deviceId);
            return Ok(eventDelays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event delays for device {DeviceId}", deviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DeviceEventDelay>> GetEventDelay(Guid id)
    {
        try
        {
            var eventDelay = await _eventDelayService.GetEventDelayByIdAsync(id);
            if (eventDelay == null)
            {
                return NotFound();
            }
            return Ok(eventDelay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event delay {EventDelayId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<DeviceEventDelay>> CreateEventDelay([FromBody] DeviceEventDelay eventDelay)
    {
        try
        {
            var created = await _eventDelayService.CreateEventDelayAsync(eventDelay);
            return CreatedAtAction(nameof(GetEventDelay), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event delay for device {DeviceId}", eventDelay.DeviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DeviceEventDelay>> UpdateEventDelay(Guid id, [FromBody] DeviceEventDelay eventDelay)
    {
        try
        {
            if (id != eventDelay.Id)
            {
                return BadRequest("ID mismatch");
            }

            var updated = await _eventDelayService.UpdateEventDelayAsync(eventDelay);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event delay {EventDelayId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEventDelay(Guid id)
    {
        try
        {
            await _eventDelayService.DeleteEventDelayAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event delay {EventDelayId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("device/{deviceId}/event/{eventType}/active")]
    public async Task<ActionResult<bool>> HasActiveEventDelay(Guid deviceId, string eventType)
    {
        try
        {
            var hasActive = await _eventDelayService.HasActiveEventDelayAsync(deviceId, eventType);
            return Ok(hasActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking active event delay for device {DeviceId}, event {EventType}", deviceId, eventType);
            return StatusCode(500, "Internal server error");
        }
    }
}
