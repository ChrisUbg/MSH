using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Services;
using MSH.Web.Data;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MSH.Web.Services;

public class DeviceGroupService : IDeviceGroupService
{
    private readonly ApplicationDbContext _context;
    private readonly MatterDeviceService _matterService;
    private readonly ILogger<DeviceGroupService> _logger;

    public DeviceGroupService(
        ApplicationDbContext context, 
        MatterDeviceService matterService,
        ILogger<DeviceGroupService> logger)
    {
        _context = context;
        _matterService = matterService;
        _logger = logger;
    }

    public async Task<List<DeviceGroup>> GetDeviceGroupsAsync()
    {
        return await _context.DeviceGroups
            .Include(g => g.DeviceGroupMembers)
                .ThenInclude(m => m.Device)
            .ToListAsync();
    }

    public async Task<DeviceGroup?> GetDeviceGroupAsync(int groupId)
    {
        return await _context.DeviceGroups
            .Include(g => g.DeviceGroupMembers)
                .ThenInclude(m => m.Device)
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

    public async Task<bool> DeleteDeviceGroupAsync(int groupId)
    {
        var group = await _context.DeviceGroups.FindAsync(groupId);
        if (group == null) return false;

        _context.DeviceGroups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddDeviceToGroupAsync(int deviceId, int groupId)
    {
        var exists = await _context.DeviceGroupMembers
            .AnyAsync(m => m.DeviceId == deviceId && m.DeviceGroupId == groupId);
        if (!exists)
        {
            _context.DeviceGroupMembers.Add(new DeviceGroupMember
            {
                DeviceId = deviceId,
                DeviceGroupId = groupId,
                CreatedById = 1, // default admin
                UpdatedById = 1
            });
            await _context.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> RemoveDeviceFromGroupAsync(int deviceId, int groupId)
    {
        var member = await _context.DeviceGroupMembers
            .FirstOrDefaultAsync(m => m.DeviceId == deviceId && m.DeviceGroupId == groupId);
        if (member != null)
        {
            _context.DeviceGroupMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> SetDevicesForGroupAsync(int groupId, List<int> deviceIds)
    {
        var group = await _context.DeviceGroups
            .Include(g => g.DeviceGroupMembers)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null) return false;

        // Remove all current members from the database and save immediately
        var existingMembers = _context.DeviceGroupMembers.Where(m => m.DeviceGroupId == groupId);
        _context.DeviceGroupMembers.RemoveRange(existingMembers);
        await _context.SaveChangesAsync();

        // Add new members
        foreach (var deviceId in deviceIds)
        {
            _context.DeviceGroupMembers.Add(new DeviceGroupMember
            {
                DeviceId = deviceId,
                DeviceGroupId = groupId,
                CreatedById = 1,
                UpdatedById = 1
            });
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<GroupHealthStatus> GetGroupHealthStatusAsync(int groupId)
    {
        var group = await GetDeviceGroupAsync(groupId);
        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return new GroupHealthStatus { IsHealthy = false, Error = "Group not found" };
        }

        var status = new GroupHealthStatus
        {
            GroupId = groupId,
            GroupName = group.Name,
            TotalDevices = group.Devices.Count,
            OnlineDevices = group.Devices.Count(d => d.Status == "online"),
            LastUpdated = DateTime.UtcNow
        };

        status.IsHealthy = status.OnlineDevices == status.TotalDevices;
        if (!status.IsHealthy)
        {
            status.Error = $"{status.TotalDevices - status.OnlineDevices} devices are offline";
        }

        return status;
    }

    public async Task<bool> SetGroupStateAsync(int groupId, string state)
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

            foreach (var device in group.Devices)
            {
                try
                {
                    if (!string.IsNullOrEmpty(device.MatterDeviceId))
                    {
                        if (state == "on")
                            _matterService.TurnOn(device.MatterDeviceId);
                        else
                            _matterService.TurnOff(device.MatterDeviceId);
                    }
                    
                    device.Status = state;
                    device.LastStateChange = DateTime.UtcNow;
                    _context.Devices.Update(device);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set state for device {DeviceId} in group {GroupId}", device.Id, groupId);
                    failCount++;
                }
            }

            await _context.SaveChangesAsync();
            
            if (failCount > 0)
            {
                _logger.LogWarning("Group {GroupId} state change: {SuccessCount} succeeded, {FailCount} failed", 
                    groupId, successCount, failCount);
            }

            return successCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set state for group {GroupId}", groupId);
            return false;
        }
    }

    public async Task<bool> SetGroupBrightnessAsync(int groupId, int brightness)
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

            foreach (var device in group.Devices.Where(d => d.DeviceType.Name == "Light"))
            {
                try
                {
                    if (!string.IsNullOrEmpty(device.MatterDeviceId))
                    {
                        // TODO: Implement Matter brightness control
                        var config = device.Configuration ?? JsonDocument.Parse("{}");
                        var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(config);
                        configDict["brightness"] = brightness;
                        device.Configuration = JsonSerializer.SerializeToDocument(configDict);
                        device.LastStateChange = DateTime.UtcNow;
                        _context.Devices.Update(device);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set brightness for device {DeviceId} in group {GroupId}", device.Id, groupId);
                    failCount++;
                }
            }

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

    public async Task<bool> SetGroupColorAsync(int groupId, string color)
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

            foreach (var device in group.Devices.Where(d => d.DeviceType.Name == "Light"))
            {
                try
                {
                    if (!string.IsNullOrEmpty(device.MatterDeviceId))
                    {
                        // TODO: Implement Matter color control
                        var config = device.Configuration ?? JsonDocument.Parse("{}");
                        var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(config);
                        configDict["color"] = color;
                        device.Configuration = JsonSerializer.SerializeToDocument(configDict);
                        device.LastStateChange = DateTime.UtcNow;
                        _context.Devices.Update(device);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set color for device {DeviceId} in group {GroupId}", device.Id, groupId);
                    failCount++;
                }
            }

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

    public async Task<Dictionary<string, object>> GetGroupStateAsync(int groupId)
    {
        var group = await GetDeviceGroupAsync(groupId);
        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return new Dictionary<string, object>();
        }

        var state = new Dictionary<string, object>();
        var deviceStates = new List<Dictionary<string, object>>();

        foreach (var device in group.Devices)
        {
            var deviceState = new Dictionary<string, object>
            {
                ["deviceId"] = device.Id,
                ["name"] = device.Name,
                ["status"] = device.Status ?? "unknown",
                ["lastStateChange"] = device.LastStateChange?.ToString("o") ?? DateTime.UtcNow.ToString("o")
            };
            deviceStates.Add(deviceState);
        }

        state["groupId"] = group.Id;
        state["name"] = group.Name;
        state["devices"] = deviceStates;
        state["lastUpdated"] = DateTime.UtcNow.ToString("o");

        return state;
    }
} 