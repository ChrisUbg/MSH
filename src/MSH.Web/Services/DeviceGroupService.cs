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
    private readonly MatterDeviceService _matterService;
    private readonly ILogger<DeviceGroupService> _logger;
    private static readonly Guid DEFAULT_USER_ID = Guid.Parse("00000000-0000-0000-0000-000000000001"); // default admin

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
                    .ThenInclude(d => d.DeviceType)
            .ToListAsync();
    }

    public async Task<DeviceGroup?> GetDeviceGroupAsync(Guid groupId)
    {
        return await _context.DeviceGroups
            .Include(g => g.DeviceGroupMembers)
                .ThenInclude(m => m.Device)
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
        var exists = await _context.DeviceGroupMembers
            .AnyAsync(m => m.DeviceId == deviceId && m.DeviceGroupId == groupId);
        if (!exists)
        {
            _context.DeviceGroupMembers.Add(new DeviceGroupMember
            {
                DeviceId = deviceId,
                DeviceGroupId = groupId,
                CreatedById = DEFAULT_USER_ID,
                UpdatedById = DEFAULT_USER_ID
            });
            await _context.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> RemoveDeviceFromGroupAsync(Guid deviceId, Guid groupId)
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

    public async Task<bool> SetDevicesForGroupAsync(Guid groupId, List<Guid> deviceIds)
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
                CreatedById = DEFAULT_USER_ID,
                UpdatedById = DEFAULT_USER_ID
            });
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<GroupHealthStatus> GetGroupHealthStatusAsync(Guid groupId)
    {
        var group = await _context.DeviceGroups
            .Include(g => g.DeviceGroupMembers)
                .ThenInclude(m => m.Device)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            _logger.LogWarning("Group {GroupId} not found", groupId);
            return new GroupHealthStatus { IsHealthy = false, Error = "Group not found" };
        }

        var devices = group.DeviceGroupMembers.Select(m => m.Device).ToList();
        var status = new GroupHealthStatus
        {
            GroupId = groupId,
            GroupName = group.Name,
            TotalDevices = devices.Count,
            OnlineDevices = devices.Count(d => d.IsOnline),
            LastUpdated = DateTime.UtcNow
        };

        status.IsHealthy = status.OnlineDevices == status.TotalDevices;
        if (!status.IsHealthy)
        {
            status.Error = $"{status.TotalDevices - status.OnlineDevices} devices are offline";
        }

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

            foreach (var device in group.Devices.Where(d => d.DeviceType?.Name == "Light"))
            {
                try
                {
                    if (!string.IsNullOrEmpty(device.MatterDeviceId))
                    {
                        // TODO: Implement Matter brightness control
                        var config = device.Configuration ?? JsonDocument.Parse("{}");
                        var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(config);
                        if (configDict != null)
                        {
                            configDict["brightness"] = brightness;
                            device.Configuration = JsonSerializer.SerializeToDocument(configDict);
                            device.LastStateChange = DateTime.UtcNow;
                            _context.Devices.Update(device);
                            successCount++;
                        }
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

            foreach (var device in group.Devices.Where(d => d.DeviceType?.Name == "Light"))
            {
                try
                {
                    if (!string.IsNullOrEmpty(device.MatterDeviceId))
                    {
                        // TODO: Implement Matter color control
                        var config = device.Configuration ?? JsonDocument.Parse("{}");
                        var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(config);
                        if (configDict != null)
                        {
                            configDict["color"] = color;
                            device.Configuration = JsonSerializer.SerializeToDocument(configDict);
                            device.LastStateChange = DateTime.UtcNow;
                            _context.Devices.Update(device);
                            successCount++;
                        }
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

        foreach (var device in group.Devices)
        {
            var deviceState = new Dictionary<string, object>
            {
                ["deviceId"] = device.Id,
                ["name"] = device.Name,
                ["status"] = device.Status,
                ["lastStateChange"] = device.LastStateChange.ToString("o")
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