using Microsoft.AspNetCore.Mvc;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Interfaces;
using System.Text.Json;

namespace MSH.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FirmwareUpdateController : ControllerBase
{
    private readonly IFirmwareUpdateService _firmwareUpdateService;
    private readonly ILogger<FirmwareUpdateController> _logger;

    public FirmwareUpdateController(IFirmwareUpdateService firmwareUpdateService, ILogger<FirmwareUpdateController> logger)
    {
        _firmwareUpdateService = firmwareUpdateService;
        _logger = logger;
    }

    // GET: api/firmwareupdate
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FirmwareUpdate>>> GetAvailableUpdates()
    {
        try
        {
            var updates = await _firmwareUpdateService.GetAvailableUpdatesAsync();
            return Ok(updates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available firmware updates");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/firmwareupdate/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FirmwareUpdate>> GetFirmwareUpdate(Guid id)
    {
        try
        {
            var firmwareUpdate = await _firmwareUpdateService.GetFirmwareUpdateByIdAsync(id);
            if (firmwareUpdate == null)
                return NotFound();
            
            return Ok(firmwareUpdate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting firmware update {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate
    [HttpPost]
    public async Task<ActionResult<FirmwareUpdate>> CreateFirmwareUpdate([FromBody] FirmwareUpdate firmwareUpdate)
    {
        try
        {
            var created = await _firmwareUpdateService.CreateFirmwareUpdateAsync(firmwareUpdate);
            return CreatedAtAction(nameof(GetFirmwareUpdate), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating firmware update");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/firmwareupdate/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFirmwareUpdate(Guid id, [FromBody] FirmwareUpdate firmwareUpdate)
    {
        try
        {
            if (id != firmwareUpdate.Id)
                return BadRequest();
            
            var updated = await _firmwareUpdateService.UpdateFirmwareUpdateAsync(firmwareUpdate);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating firmware update {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/firmwareupdate/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFirmwareUpdate(Guid id)
    {
        try
        {
            var result = await _firmwareUpdateService.DeleteFirmwareUpdateAsync(id);
            if (!result)
                return NotFound();
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting firmware update {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/firmwareupdate/device/{deviceId}
    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<IEnumerable<DeviceFirmwareUpdate>>> GetDeviceUpdates(Guid deviceId)
    {
        try
        {
            var updates = await _firmwareUpdateService.GetDeviceUpdatesAsync(deviceId);
            return Ok(updates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device updates for device {DeviceId}", deviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/firmwareupdate/search/{deviceId}
    [HttpGet("search/{deviceId:guid}")]
    public async Task<ActionResult<IEnumerable<FirmwareUpdate>>> SearchForUpdates(Guid deviceId)
    {
        try
        {
            var updates = await _firmwareUpdateService.SearchForUpdatesAsync(deviceId);
            return Ok(updates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for updates for device {DeviceId}", deviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate/check/{deviceId}
    [HttpPost("check/{deviceId:guid}")]
    public async Task<ActionResult<bool>> CheckForUpdates(Guid deviceId)
    {
        try
        {
            var hasUpdates = await _firmwareUpdateService.CheckForUpdatesAsync(deviceId);
            return Ok(hasUpdates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for updates for device {DeviceId}", deviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate/download/{deviceUpdateId}
    [HttpPost("download/{deviceUpdateId:guid}")]
    public async Task<IActionResult> StartDownload(Guid deviceUpdateId)
    {
        try
        {
            var result = await _firmwareUpdateService.StartDownloadAsync(deviceUpdateId);
            if (!result)
                return BadRequest("Cannot start download for this device update");
            
            return Ok(new { message = "Download started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting download for device update {Id}", deviceUpdateId);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/firmwareupdate/download/{deviceUpdateId}
    [HttpDelete("download/{deviceUpdateId:guid}")]
    public async Task<IActionResult> CancelDownload(Guid deviceUpdateId)
    {
        try
        {
            var result = await _firmwareUpdateService.CancelDownloadAsync(deviceUpdateId);
            if (!result)
                return BadRequest("Cannot cancel download for this device update");
            
            return Ok(new { message = "Download cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling download for device update {Id}", deviceUpdateId);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/firmwareupdate/progress/{deviceUpdateId}
    [HttpGet("progress/{deviceUpdateId:guid}")]
    public async Task<ActionResult<long>> GetDownloadProgress(Guid deviceUpdateId)
    {
        try
        {
            var progress = await _firmwareUpdateService.GetDownloadProgressAsync(deviceUpdateId);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download progress for device update {Id}", deviceUpdateId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate/install/{deviceUpdateId}
    [HttpPost("install/{deviceUpdateId:guid}")]
    public async Task<IActionResult> StartInstallation(Guid deviceUpdateId)
    {
        try
        {
            var result = await _firmwareUpdateService.StartInstallationAsync(deviceUpdateId);
            if (!result)
                return BadRequest("Cannot start installation for this device update");
            
            return Ok(new { message = "Installation started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting installation for device update {Id}", deviceUpdateId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate/confirm/{deviceUpdateId}
    [HttpPost("confirm/{deviceUpdateId:guid}")]
    public async Task<IActionResult> ConfirmInstallation(Guid deviceUpdateId, [FromBody] ConfirmInstallationRequest request)
    {
        try
        {
            var result = await _firmwareUpdateService.ConfirmInstallationAsync(deviceUpdateId, request.ConfirmedBy);
            if (!result)
                return BadRequest("Cannot confirm installation for this device update");
            
            return Ok(new { message = "Installation confirmed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming installation for device update {Id}", deviceUpdateId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate/test/{deviceUpdateId}
    [HttpPost("test/{deviceUpdateId:guid}")]
    public async Task<IActionResult> StartTesting(Guid deviceUpdateId)
    {
        try
        {
            var result = await _firmwareUpdateService.StartTestingAsync(deviceUpdateId);
            if (!result)
                return BadRequest("Cannot start testing for this device update");
            
            return Ok(new { message = "Testing started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting testing for device update {Id}", deviceUpdateId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate/test/{deviceUpdateId}/complete
    [HttpPost("test/{deviceUpdateId:guid}/complete")]
    public async Task<IActionResult> CompleteTesting(Guid deviceUpdateId, [FromBody] CompleteTestingRequest request)
    {
        try
        {
            JsonDocument? testResults = null;
            if (!string.IsNullOrEmpty(request.TestResults))
            {
                testResults = JsonDocument.Parse(request.TestResults);
            }
            
            var result = await _firmwareUpdateService.CompleteTestingAsync(deviceUpdateId, request.TestPassed, testResults);
            if (!result)
                return BadRequest("Cannot complete testing for this device update");
            
            return Ok(new { message = "Testing completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing testing for device update {Id}", deviceUpdateId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate/rollback/{deviceUpdateId}
    [HttpPost("rollback/{deviceUpdateId:guid}")]
    public async Task<IActionResult> StartRollback(Guid deviceUpdateId, [FromBody] RollbackRequest request)
    {
        try
        {
            var result = await _firmwareUpdateService.StartRollbackAsync(deviceUpdateId, request.Reason);
            if (!result)
                return BadRequest("Cannot start rollback for this device update");
            
            return Ok(new { message = "Rollback started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting rollback for device update {Id}", deviceUpdateId);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/firmwareupdate/batch
    [HttpPost("batch")]
    public async Task<IActionResult> StartBatchUpdate([FromBody] BatchUpdateRequest request)
    {
        try
        {
            var result = await _firmwareUpdateService.StartBatchUpdateAsync(request.DeviceIds, request.FirmwareUpdateId);
            if (!result)
                return BadRequest("Cannot start batch update");
            
            return Ok(new { message = "Batch update started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting batch update");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/firmwareupdate/pending
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<DeviceFirmwareUpdate>>> GetPendingUpdates()
    {
        try
        {
            var updates = await _firmwareUpdateService.GetPendingUpdatesAsync();
            return Ok(updates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending updates");
            return StatusCode(500, "Internal server error");
        }
    }
}

// Request models
public class ConfirmInstallationRequest
{
    public string ConfirmedBy { get; set; } = string.Empty;
}

public class CompleteTestingRequest
{
    public bool TestPassed { get; set; }
    public string? TestResults { get; set; }
}

public class RollbackRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class BatchUpdateRequest
{
    public IEnumerable<Guid> DeviceIds { get; set; } = new List<Guid>();
    public Guid FirmwareUpdateId { get; set; }
}
