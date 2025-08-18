using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Data;

namespace MSH.Infrastructure.Services;

public class GroupStateManager : IGroupStateManager
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GroupStateManager> _logger;
    private readonly MatterDeviceService _matterService;

    public GroupStateManager(
        ApplicationDbContext context,
        ILogger<GroupStateManager> logger,
        MatterDeviceService matterService)
    {
        _context = context;
        _logger = logger;
        _matterService = matterService;
    }

    public async Task<bool> SynchronizeGroupStateAsync(Guid groupId)
    {
        try
        {
            var group = await _context.DeviceGroups
                // .Include(g => g.DeviceGroupMembers)
                // .ThenInclude(dgm => dgm.Device)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found", groupId);
                return false;
            }

            var deviceStates = new List<Dictionary<string, object>>();
            // foreach (var deviceGroupMember in group.DeviceGroupMembers)
            // {
            //     var device = deviceGroupMember.Device;
            //     try
            //     {
            //         if (!string.IsNullOrEmpty(device.MatterDeviceId))
            //         {
            //             // Get current state from Matter device
            //             var matterState = new Dictionary<string, object>
            //             {
            //                 ["status"] = device.Status,
            //                 ["configuration"] = device.Configuration?.Deserialize<Dictionary<string, object>>() ?? new Dictionary<string, object>()
            //             };
            //             
            //             device.Status = matterState["status"].ToString() ?? "unknown";
            //             device.Configuration = JsonSerializer.SerializeToDocument(matterState["configuration"] ?? new Dictionary<string, object>());
            //             device.LastStateChange = DateTime.UtcNow;
            //             _context.Devices.Update(device);
            //             deviceStates.Add(matterState);
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.LogError(ex, "Failed to synchronize state for device {DeviceId}", device.Id);
            //     }
            // }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize group state for group {GroupId}", groupId);
            return false;
        }
    }

    public async Task<bool> ValidateGroupStateAsync(Guid groupId, Dictionary<string, object> state)
    {
        try
        {
            var group = await _context.DeviceGroups
                // .Include(g => g.DeviceGroupMembers)
                // .ThenInclude(dgm => dgm.Device)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found", groupId);
                return false;
            }

            // Validate state properties
            if (state.ContainsKey("isOn"))
            {
                if (state["isOn"] is not bool)
                {
                    _logger.LogWarning("Invalid isOn value for group {GroupId}", groupId);
                    return false;
                }
            }

            if (state.ContainsKey("brightness"))
            {
                if (state["brightness"] is not int brightness || brightness < 0 || brightness > 100)
                {
                    _logger.LogWarning("Invalid brightness value for group {GroupId}", groupId);
                    return false;
                }
            }

            if (state.ContainsKey("color"))
            {
                if (state["color"] is not string color || !IsValidColor(color))
                {
                    _logger.LogWarning("Invalid color value for group {GroupId}", groupId);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate group state for group {GroupId}", groupId);
            return false;
        }
    }

    public async Task<bool> PersistGroupStateAsync(Guid groupId, Dictionary<string, object> state)
    {
        try
        {
            var group = await _context.DeviceGroups
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found", groupId);
                return false;
            }

            // Update group state
            var groupState = new GroupState
            {
                GroupId = groupId,
                State = JsonSerializer.SerializeToDocument(state),
                LastUpdated = DateTime.UtcNow
            };

            _context.GroupStates.Add(groupState);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist group state for group {GroupId}", groupId);
            return false;
        }
    }

    public async Task NotifyStateChangeAsync(Guid groupId, Dictionary<string, object> oldState, Dictionary<string, object> newState)
    {
        try
        {
            var group = await _context.DeviceGroups
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found", groupId);
                return;
            }

            // Log state change
            _logger.LogInformation("Group {GroupId} state changed from {OldState} to {NewState}", 
                groupId, JsonSerializer.Serialize(oldState), JsonSerializer.Serialize(newState));

            // Here you could add notification logic (email, push notification, etc.)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify state change for group {GroupId}", groupId);
        }
    }

    public async Task<Dictionary<string, object>> GetGroupStateHistoryAsync(Guid groupId, int limit = 10)
    {
        try
        {
            var groupStates = await _context.GroupStates
                .Where(gs => gs.GroupId == groupId)
                .OrderByDescending(gs => gs.LastUpdated)
                .Take(limit)
                .ToListAsync();

            var history = new Dictionary<string, object>();
            foreach (var state in groupStates)
            {
                var stateData = state.State.Deserialize<Dictionary<string, object>>() ?? new Dictionary<string, object>();
                history[state.LastUpdated.ToString("O")] = stateData;
            }

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get group state history for group {GroupId}", groupId);
            return new Dictionary<string, object>();
        }
    }

    private bool IsValidColor(string color)
    {
        // Simple color validation - could be enhanced
        return !string.IsNullOrEmpty(color) && color.Length <= 50;
    }
} 