using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using MSH.Core.Entities;
using MSH.Core.Interfaces;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Models;
using MSH.Infrastructure.Services;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MSH.Infrastructure.Entities;
using MSH.Web.Data;
using MSH.Web.Services;

namespace MSH.Tests.Services;

public class DeviceGroupServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<DeviceGroupService>> _loggerMock;
    private readonly Mock<MatterDeviceService> _matterServiceMock;
    private readonly DeviceGroupService _service;
    private readonly int _testGroupId = 1;

    public DeviceGroupServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DeviceGroupService>>();
        _matterServiceMock = new Mock<MatterDeviceService>();
        _service = new DeviceGroupService(_context, _matterServiceMock.Object, _loggerMock.Object);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Add device types
        var lightType = new DeviceType { Name = "Light" };
        var sensorType = new DeviceType { Name = "Sensor" };
        _context.DeviceTypes.AddRange(lightType, sensorType);

        // Add devices
        var devices = new List<MSH.Infrastructure.Entities.Device>
        {
            new()
            {
                Name = "Test Device 1",
                DeviceType = lightType,
                Status = "online",
                MatterDeviceId = "device-1",
                Configuration = JsonDocument.Parse(JsonSerializer.Serialize(new { brightness = 50, color = "white" })),
                LastStateChange = DateTime.UtcNow
            },
            new()
            {
                Name = "Test Device 2",
                DeviceType = lightType,
                Status = "online",
                MatterDeviceId = "device-2",
                Configuration = JsonDocument.Parse(JsonSerializer.Serialize(new { brightness = 75, color = "warm" })),
                LastStateChange = DateTime.UtcNow
            },
            new()
            {
                Name = "Test Device 3",
                DeviceType = sensorType,
                Status = "offline",
                MatterDeviceId = "device-3",
                Configuration = JsonDocument.Parse(JsonSerializer.Serialize(new { threshold = 25 })),
                LastStateChange = DateTime.UtcNow
            }
        };

        _context.Devices.AddRange(devices);

        // Add device group
        var group = new DeviceGroup
        {
            Id = _testGroupId,
            Name = "Test Group",
            Description = "Test Description",
            DeviceGroupMembers = devices.Select(d => new DeviceGroupMember
            {
                Device = d,
                CreatedById = 1,
                UpdatedById = 1
            }).ToList()
        };

        _context.DeviceGroups.Add(group);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetGroupHealthStatusAsync_WithValidGroup_ReturnsCorrectStatus()
    {
        // Act
        var status = await _service.GetGroupHealthStatusAsync(_testGroupId);

        // Assert
        Assert.NotNull(status);
        Assert.Equal(3, status.TotalDevices);
        Assert.Equal(2, status.OnlineDevices);
        Assert.False(status.IsHealthy);
        Assert.NotNull(status.Error);
        Assert.Contains("1 devices are offline", status.Error);
    }

    [Fact]
    public async Task GetGroupHealthStatusAsync_WithInvalidGroup_ReturnsUnhealthyStatus()
    {
        // Act
        var status = await _service.GetGroupHealthStatusAsync(999);

        // Assert
        Assert.NotNull(status);
        Assert.False(status.IsHealthy);
        Assert.Equal("Group not found", status.Error);
    }

    [Fact]
    public async Task SetGroupStateAsync_WithValidGroup_UpdatesAllDevices()
    {
        // Act
        var result = await _service.SetGroupStateAsync(_testGroupId, "on");

        // Assert
        Assert.True(result);
        var updatedDevices = await _context.Devices
            .Where(d => d.DeviceGroupMembers.Any(m => m.DeviceGroupId == _testGroupId))
            .ToListAsync();
        Assert.All(updatedDevices, d => Assert.Equal("on", d.Status));
    }

    [Fact]
    public async Task SetGroupStateAsync_WithInvalidGroup_ReturnsFalse()
    {
        // Act
        var result = await _service.SetGroupStateAsync(999, "on");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetGroupBrightnessAsync_WithValidGroup_UpdatesLightDevices()
    {
        // Act
        var result = await _service.SetGroupBrightnessAsync(_testGroupId, 75);

        // Assert
        Assert.True(result);
        var updatedDevices = await _context.Devices
            .Where(d => d.DeviceGroupMembers.Any(m => m.DeviceGroupId == _testGroupId) 
                     && d.DeviceType.Name == "Light")
            .ToListAsync();
        Assert.All(updatedDevices, d => 
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(d.Configuration.RootElement.GetRawText());
            Assert.Equal(75, config["brightness"]);
        });
    }

    [Fact]
    public async Task GetGroupStateAsync_WithValidGroup_ReturnsAllDeviceStates()
    {
        // Act
        var states = await _service.GetGroupStateAsync(_testGroupId);

        // Assert
        Assert.NotNull(states);
        Assert.Equal(4, states.Count); // groupId, name, devices, lastUpdated
        Assert.True(states.ContainsKey("devices"));
        var devices = states["devices"] as List<Dictionary<string, object>>;
        Assert.NotNull(devices);
        Assert.Equal(3, devices.Count);
        Assert.All(devices, device => 
        {
            Assert.True(device.ContainsKey("status"));
            Assert.True(device.ContainsKey("name"));
            Assert.True(device.ContainsKey("deviceId"));
            Assert.True(device.ContainsKey("lastStateChange"));
        });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 