using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using MSH.Infrastructure.Entities;
using MSH.Web.Data;

namespace MSH.Web.Services;

public class DeviceGroupService : IDeviceGroupService
{
    private readonly ApplicationDbContext _context;

    public DeviceGroupService(ApplicationDbContext context)
    {
        _context = context;
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
} 