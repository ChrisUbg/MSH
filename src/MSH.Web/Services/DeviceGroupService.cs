using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Services;
using MSH.Infrastructure.Data;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MSH.Web.Services;

public class DeviceGroupService : IDeviceGroupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceGroupService> _logger;
    private static readonly string DEFAULT_USER_ID = "bb1be326-f26e-4684-bbf5-5c3df450dc61"; // system user

    public DeviceGroupService(
        ApplicationDbContext context, 
        ILogger<DeviceGroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<DeviceGroup>> GetDeviceGroupsAsync()
    {
        return await _context.DeviceGroups
            .Include(g => g.Devices)
                .ThenInclude(d => d.DeviceType)
            .ToListAsync();
    }

    public async Task<DeviceGroup?> GetDeviceGroupAsync(Guid groupId)
    {
        return await _context.DeviceGroups
            .Include(g => g.Devices)
                .ThenInclude(d => d.DeviceType)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<DeviceGroup> AddDeviceGroupAsync(DeviceGroup group)
    {
        _context.DeviceGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<DeviceGroup> UpdateDeviceGroupAsync(DeviceGroup group)
    {
        _context.DeviceGroups.Update(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<bool> DeleteDeviceGroupAsync(Guid groupId)
    {
        var group = await _context.DeviceGroups.FindAsync(groupId);
        if (group == null) return false;

        _context.DeviceGroups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddDeviceToGroupAsync(Guid deviceId, Guid groupId)
    {
        // var exists = await _context.DeviceGroupMembers
        //     .AnyAsync(m => m.DeviceId == deviceId && m.DeviceGroupId == groupId);
        // if (!exists)
        // {
        //     _context.DeviceGroupMembers.Add(new DeviceGroupMember
        //     {
        //         DeviceId = deviceId,
        //         DeviceGroupId = groupId,
        //         CreatedById = DEFAULT_USER_ID,
        //         UpdatedById = DEFAULT_USER_ID
        //     });
        //     await _context.SaveChangesAsync();
        // }
        return true;
    }

    public async Task<bool> RemoveDeviceFromGroupAsync(Guid deviceId, Guid groupId)
    {
        // var member = await _context.DeviceGroupMembers
        //     .FirstOrDefaultAsync(m => m.DeviceId == deviceId && m.DeviceGroupId == groupId);
        // if (member != null)
        // {
        //     _context.DeviceGroupMembers.Remove(member);
        //     await _context.SaveChangesAsync();
        // }
        return true;
    }

    public async Task<bool> SetDevicesForGroupAsync(Guid groupId, List<Guid> deviceIds)
    {
        var group = await _context.DeviceGroups
            .Include(g => g.Devices)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null) return false;

        // Get the devices to add
        var devicesToAdd = await _context.Devices
            .Where(d => deviceIds.Contains(d.Id))
            .ToListAsync();

        // Clear existing devices and add new ones
        group.Devices.Clear();
        foreach (var device in devicesToAdd)
        {
            group.Devices.Add(device);
        }

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated device group {GroupId} with {DeviceCount} devices", groupId, devicesToAdd.Count);
        return true;
    }

    public async Task<GroupHealthStatus> GetGroupHealthStatusAsync(Guid groupId)
    {
        var group = await _context.DeviceGroups
            // .Include(g => g.DeviceGroupMembers)
            //     .ThenInclude(m => m.Device)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return new GroupHealthStatus { IsHealthy = false, Error = "Group not found" };
        }

        // var devices = group.DeviceGroupMembers.Select(m => m.Device).ToList();
        var status = new GroupHealthStatus
        {
            GroupId = groupId,
            GroupName = group.Name,
            TotalDevices = 0, // devices.Count,
            OnlineDevices = 0, // devices.Count(d => d.IsOnline),
            LastUpdated = DateTime.UtcNow
        };

        status.IsHealthy = true; // status.OnlineDevices == status.TotalDevices;
        // if (!status.IsHealthy)
        // {
        //     status.Error = $"{status.TotalDevices - status.OnlineDevices} devices are offline";
        // }

        return status;
    }

    public async Task<bool> SetGroupStateAsync(Guid groupId, string state)
    {
        try
        {
            var group = await GetDeviceGroupAsync(groupId);
            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found", groupId);
                return false;
            }

            var successCount = 0;
            var failCount = 0;

            // foreach (var deviceGroupMember in group.DeviceGroupMembers)
            // {
            //     var device = deviceGroupMember.Device;
            //     try
            //     {
            //         if (!string.IsNullOrEmpty(device.MatterDeviceId))
            //         {
            //             if (state == "on")
            //                 _matterService.TurnOn(device.MatterDeviceId);
            //             else
            //                 _matterService.TurnOff(device.MatterDeviceId);
            //         }
            //         
            //         device.Status = state;
            //         device.LastStateChange = DateTime.UtcNow;
            //         _context.Devices.Update(device);
            //         successCount++;
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.LogError(ex, "Failed to set state for device {DeviceId} in group {GroupId}", device.Id, groupId);
            //         failCount++;
            //     }
            // }

            await _context.SaveChangesAsync();
            
            if (failCount > 0)
            {
                _logger.LogWarning("Group {GroupId} state change: {SuccessCount} succeeded, {FailCount} failed", 
                    groupId, successCount, failCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set state for group {GroupId}", groupId);
            return false;
        }
        return true;
    }

    public async Task<bool> SetGroupBrightnessAsync(Guid groupId, int brightness)
    {
        try
        {
            var group = await GetDeviceGroupAsync(groupId);
            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found", groupId);
                return false;
            }

            var successCount = 0;
            var failCount = 0;

            // foreach (var deviceGroupMember in group.DeviceGroupMembers.Where(dgm => dgm.Device.DeviceType?.Name == "Light"))
            // {
            //     var device = deviceGroupMember.Device;
            //     try
            //     {
            //         if (!string.IsNullOrEmpty(device.MatterDeviceId))
            //         {
            //             // TODO: Implement Matter brightness control
            //             var config = device.Configuration ?? JsonDocument.Parse("{}");
            //             var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(config);
            //             if (configDict != null)
            //             {
            //                 configDict["brightness"] = brightness;
            //                 device.Configuration = JsonSerializer.SerializeToDocument(configDict);
            //                 device.LastStateChange = DateTime.UtcNow;
            //                 _context.Devices.Update(device);
            //                 successCount++;
            //             }
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.LogError(ex, "Failed to set brightness for device {DeviceId} in group {GroupId}", device.Id, groupId);
            //         failCount++;
            //     }
            // }

            await _context.SaveChangesAsync();
            
            if (failCount > 0)
            {
                _logger.LogWarning("Group {GroupId} brightness change: {SuccessCount} succeeded, {FailCount} failed", 
                    groupId, successCount, failCount);
            }

            return successCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set brightness for group {GroupId}", groupId);
            return false;
        }
    }

    public async Task<bool> SetGroupColorAsync(Guid groupId, string color)
    {
        try
        {
            var group = await GetDeviceGroupAsync(groupId);
            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found", groupId);
                return false;
            }

            var successCount = 0;
            var failCount = 0;

            // foreach (var deviceGroupMember in group.DeviceGroupMembers.Where(dgm => dgm.Device.DeviceType?.Name == "Light"))
            // {
            //     var device = deviceGroupMember.Device;
            //     try
            //     {
            //         if (!string.IsNullOrEmpty(device.MatterDeviceId))
            //         {
            //             // TODO: Implement Matter color control
            //             var config = device.Configuration ?? JsonDocument.Parse("{}");
            //             var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(config);
            //             if (configDict != null)
            //             {
            //                 configDict["color"] = color;
            //                 device.Configuration = JsonSerializer.SerializeToDocument(configDict);
            //                 device.LastStateChange = DateTime.UtcNow;
            //                 _context.Devices.Update(device);
            //                 successCount++;
            //             }
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.LogError(ex, "Failed to set color for device {DeviceId} in group {GroupId}", device.Id, groupId);
            //         failCount++;
            //     }
            // }

            await _context.SaveChangesAsync();
            
            if (failCount > 0)
            {
                _logger.LogWarning("Group {GroupId} color change: {SuccessCount} succeeded, {FailCount} failed", 
                    groupId, successCount, failCount);
            }

            return successCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set color for group {GroupId}", groupId);
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetGroupStateAsync(Guid groupId)
    {
        var group = await GetDeviceGroupAsync(groupId);
        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return new Dictionary<string, object>();
        }

        var state = new Dictionary<string, object>();
        var deviceStates = new List<Dictionary<string, object>>();

        // foreach (var deviceGroupMember in group.DeviceGroupMembers)
        // {
        //     var device = deviceGroupMember.Device;
        //     var deviceState = new Dictionary<string, object>
        //     {
        //         ["deviceId"] = device.Id,
        //         ["name"] = device.Name,
        //         ["status"] = device.Status,
        //         ["lastStateChange"] = device.LastStateChange.ToString("o")
        //     };
        //     deviceStates.Add(deviceState);
        // }

        state["groupId"] = group.Id;
        state["name"] = group.Name;
        state["devices"] = deviceStates;
        state["lastUpdated"] = DateTime.UtcNow.ToString("o");

        return state;
    }

    public async Task<DeviceGroup> GetDeviceGroupByNameAsync(string name)
    {
        var result = await _context.DeviceGroups
            .Include(g => g.DeviceTypes)
            // .Include(g => g.DeviceGroupMembers)
            //     .ThenInclude(m => m.Device)
            //         .ThenInclude(d => d.DeviceType)
            .FirstOrDefaultAsync(g => g.Name == name);

        if (result is null)
        {
            return new DeviceGroup();
        }
        return result;
    }

    public async Task<List<DeviceType>> GetDeviceTypesByGroupAsync(Guid groupId)
    {
        // Temporarily disabled due to Entity Framework query generation issues
        // return await _context.DeviceTypes
        //     .Where(dt => dt.DeviceGroupId == groupId)
        //     .OrderBy(dt => dt.Name)
        //     .ToListAsync();
        return new List<DeviceType>();
    }

    public async Task<List<DeviceGroup>> GetDeviceGroupsWithDeviceTypesAsync()
    {
        return await _context.DeviceGroups
            .Include(g => g.DeviceTypes)
            .Where(g => g.IsActive)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<Dictionary<string, object>> GetDeviceCapabilitiesAsync(Guid deviceTypeId)
    {
        var deviceType = await _context.DeviceTypes
            .Include(dt => dt.DeviceGroup)
            .FirstOrDefaultAsync(dt => dt.Id == deviceTypeId);

        if (deviceType == null)
            return new Dictionary<string, object>();

        var capabilities = new Dictionary<string, object>();

        // Add device type capabilities
        if (deviceType.Capabilities != null)
        {
            var typeCapabilities = JsonSerializer.Deserialize<Dictionary<string, object>>(deviceType.Capabilities.RootElement.ToString());
            foreach (var capability in typeCapabilities)
            {
                capabilities[capability.Key] = capability.Value;
            }
        }

        // Add group default capabilities
        if (deviceType.DeviceGroup?.DefaultCapabilities != null)
        {
            var groupCapabilities = JsonSerializer.Deserialize<Dictionary<string, object>>(deviceType.DeviceGroup.DefaultCapabilities.RootElement.ToString());
            foreach (var capability in groupCapabilities)
            {
                if (!capabilities.ContainsKey(capability.Key))
                {
                    capabilities[capability.Key] = capability.Value;
                }
            }
        }

        return capabilities;
    }

    public async Task<string> GetDeviceIconAsync(string deviceTypeName)
    {
        if (string.IsNullOrEmpty(deviceTypeName))
            return "oi-device-hdd";

        var deviceType = await _context.DeviceTypes
            .Include(dt => dt.DeviceGroup)
            .FirstOrDefaultAsync(dt => dt.Name == deviceTypeName);

        if (deviceType?.DeviceGroup != null)
            return deviceType.DeviceGroup.Icon;

        // Default icons based on device type name
        return deviceTypeName.ToLower() switch
        {
            var s when s.Contains("socket") || s.Contains("plug") => "oi-plug",
            var s when s.Contains("bulb") || s.Contains("light") => "oi-lightbulb",
            var s when s.Contains("sensor") => "oi-monitor",
            var s when s.Contains("switch") => "oi-toggle-on",
            var s when s.Contains("thermostat") => "oi-thermometer",
            var s when s.Contains("camera") => "oi-camera",
            var s when s.Contains("lock") => "oi-lock-locked",
            _ => "oi-device-hdd"
        };
    }

    public async Task<CommissioningMethod> GetPreferredCommissioningMethodAsync(Guid deviceTypeId)
    {
        var deviceType = await _context.DeviceTypes
            .Include(dt => dt.DeviceGroup)
            .FirstOrDefaultAsync(dt => dt.Id == deviceTypeId);

        if (deviceType?.DeviceGroup?.PreferredCommissioningMethod != null)
        {
            return Enum.Parse<CommissioningMethod>(deviceType.DeviceGroup.PreferredCommissioningMethod);
        }

        // Default to BLE-WiFi for most devices
        return CommissioningMethod.BLE_WiFi;
    }

    public async Task EnsureDefaultDeviceGroupsAsync()
    {
        // Check if we already have device groups
        if (await _context.DeviceGroups.AnyAsync())
            return;

        var defaultGroups = new List<DeviceGroup>
        {
            new DeviceGroup
            {
                Name = "Smart Plugs & Sockets",
                Description = "Power control devices for appliances and lighting",
                Icon = "oi-plug",
                PreferredCommissioningMethod = "BLE_WiFi",
                SortOrder = 1,
                DefaultCapabilities = JsonDocument.Parse(@"{
                    ""onOff"": true,
                    ""powerMonitoring"": true,
                    ""maxPower"": 3680
                }"),
                CreatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61",
                UpdatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61"
            },
            new DeviceGroup
            {
                Name = "Smart Lighting",
                Description = "LED bulbs, strips, and lighting controls",
                Icon = "oi-lightbulb",
                PreferredCommissioningMethod = "BLE_WiFi",
                SortOrder = 2,
                DefaultCapabilities = JsonDocument.Parse(@"{
                    ""onOff"": true,
                    ""brightness"": true,
                    ""colorTemperature"": true,
                    ""color"": true
                }"),
                CreatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61",
                UpdatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61"
            },
            new DeviceGroup
            {
                Name = "Smart Sensors",
                Description = "Environmental and security sensors",
                Icon = "oi-monitor",
                PreferredCommissioningMethod = "BLE_WiFi",
                SortOrder = 3,
                DefaultCapabilities = JsonDocument.Parse(@"{
                    ""temperature"": true,
                    ""humidity"": true,
                    ""motion"": true,
                    ""contact"": true
                }"),
                CreatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61",
                UpdatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61"
            },
            new DeviceGroup
            {
                Name = "Smart Switches",
                Description = "Wall switches and dimmers",
                Icon = "oi-toggle-on",
                PreferredCommissioningMethod = "BLE_WiFi",
                SortOrder = 4,
                DefaultCapabilities = JsonDocument.Parse(@"{
                    ""onOff"": true,
                    ""brightness"": true,
                    ""dimmer"": true
                }"),
                CreatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61",
                UpdatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61"
            },
            new DeviceGroup
            {
                Name = "Other Devices",
                Description = "Other Matter-compatible devices",
                Icon = "oi-device-hdd",
                PreferredCommissioningMethod = "BLE_WiFi",
                SortOrder = 5,
                DefaultCapabilities = JsonDocument.Parse(@"{
                    ""onOff"": true
                }"),
                CreatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61",
                UpdatedById = "bb1be326-f26e-4684-bbf5-5c3df450dc61"
            }
        };

        _context.DeviceGroups.AddRange(defaultGroups);
        await _context.SaveChangesAsync();

        // Update existing device types to assign them to groups
        await AssignDeviceTypesToGroupsAsync();
    }

    private async Task AssignDeviceTypesToGroupsAsync()
    {
        var deviceTypes = await _context.DeviceTypes.ToListAsync();
        var groups = await _context.DeviceGroups.ToListAsync();

        foreach (var deviceType in deviceTypes)
        {
            var group = deviceType.Name.ToLower() switch
            {
                var s when s.Contains("socket") || s.Contains("plug") => groups.FirstOrDefault(g => g.Name == "Smart Plugs & Sockets"),
                var s when s.Contains("bulb") || s.Contains("light") => groups.FirstOrDefault(g => g.Name == "Smart Lighting"),
                var s when s.Contains("sensor") => groups.FirstOrDefault(g => g.Name == "Smart Sensors"),
                var s when s.Contains("switch") => groups.FirstOrDefault(g => g.Name == "Smart Switches"),
                _ => groups.FirstOrDefault(g => g.Name == "Other Devices")
            };

            if (group != null)
            {
                deviceType.DeviceGroupId = group.Id;
            }
        }

        await _context.SaveChangesAsync();
    }
} 