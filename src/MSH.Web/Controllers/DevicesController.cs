using Microsoft.AspNetCore.Mvc;
using MSH.Infrastructure.Entities;
using MSH.Web.Services;
using System.Text.Json;

namespace MSH.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceTypeService _deviceTypeService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(
        IDeviceService deviceService,
        IDeviceTypeService deviceTypeService,
        ILogger<DevicesController> logger)
    {
        _deviceService = deviceService;
        _deviceTypeService = deviceTypeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceRequest request)
    {
        try
        {
            _logger.LogInformation($"Creating device: {request.Name} with MatterDeviceId: {request.MatterDeviceId}");

            // Get or create device type
            var deviceTypes = await _deviceTypeService.GetDeviceTypesAsync();
            var deviceType = deviceTypes.FirstOrDefault(dt => dt.Id.ToString() == request.DeviceTypeId);
            
            if (deviceType == null)
            {
                // Create a default NOUS A8M device type if not found
                deviceType = new DeviceType
                {
                    Name = "NOUS A8M Socket",
                    Description = "NOUS A8M 16A Smart Socket with Matter support",
                    Capabilities = JsonDocument.Parse(@"{
                        ""onOff"": true,
                        ""powerMonitoring"": true,
                        ""energyMonitoring"": true,
                        ""maxPower"": 3680
                    }"),
                    IsSimulated = false,
                    CreatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61"
                };
                await _deviceTypeService.AddDeviceTypeAsync(deviceType);
            }

            var device = new Device
            {
                Name = request.Name,
                Description = $"Commissioned via Matter Bridge",
                MatterDeviceId = request.MatterDeviceId,
                DeviceTypeId = deviceType.Id,
                RoomId = string.IsNullOrEmpty(request.RoomId) ? null : Guid.Parse(request.RoomId),
                Status = request.Status ?? "Online",
                IsOnline = request.IsOnline,
                LastSeen = DateTime.UtcNow,
                LastStateChange = DateTime.UtcNow,
                Properties = JsonDocument.Parse(JsonSerializer.Serialize(request.Properties ?? new Dictionary<string, object>())),
                Configuration = JsonDocument.Parse("{}"),
                CreatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61",
                UpdatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61"
            };

            var createdDevice = await _deviceService.AddDeviceAsync(device);
            
            _logger.LogInformation($"Device created successfully with ID: {createdDevice.Id}");

            return Ok(new
            {
                Id = createdDevice.Id,
                Name = createdDevice.Name,
                MatterDeviceId = createdDevice.MatterDeviceId,
                Status = "Created"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create device: {request.Name}");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDevices()
    {
        try
        {
            var devices = await _deviceService.GetDevicesAsync();
            return Ok(devices.Select(d => new
            {
                d.Id,
                d.Name,
                d.MatterDeviceId,
                d.Status,
                d.IsOnline,
                DeviceType = d.DeviceType?.Name,
                Room = d.Room?.Name
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get devices");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("test-matter-bridge")]
    public async Task<IActionResult> TestMatterBridge()
    {
        try
        {
            return Ok(new { 
                message = "MatterBridge removed - device control now handled directly via chip-tool on Pi",
                status = "success"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}

public class CreateDeviceRequest
{
    public string Name { get; set; } = null!;
    public string MatterDeviceId { get; set; } = null!;
    public string? DeviceTypeId { get; set; }
    public string? RoomId { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    public string? Status { get; set; }
    public bool IsOnline { get; set; } = true;
} 