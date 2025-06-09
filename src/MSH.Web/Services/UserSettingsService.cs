using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly ApplicationDbContext _context;

    public UserSettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings> GetUserSettingsAsync(Guid userId)
    {
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

        if (settings == null)
        {
            // Create default settings if none exist
            settings = new UserSettings
            {
                UserId = userId,
                Theme = "light",
                Language = "en",
                ShowOfflineDevices = true,
                DefaultView = "rooms",
                EmailNotifications = true,
                PushNotifications = true,
                ShowEmptyRooms = true,
                ShowAutomationSuggestions = true
            };

            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<UserSettings> UpdateUserSettingsAsync(Guid userId, UserSettings settings)
    {
        var existingSettings = await GetUserSettingsAsync(userId);
        
        // Update all properties
        _context.Entry(existingSettings).CurrentValues.SetValues(settings);
        existingSettings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existingSettings;
    }

    public async Task<UserSettings> UpdateThemeAsync(Guid userId, string theme)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.Theme = theme;
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateLanguageAsync(Guid userId, string language)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.Language = language;
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateDashboardLayoutAsync(Guid userId, object layout)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.DashboardLayout = JsonSerializer.SerializeToDocument(layout);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateNotificationPreferencesAsync(Guid userId, object preferences)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.NotificationPreferences = JsonSerializer.SerializeToDocument(preferences);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> AddFavoriteDeviceAsync(Guid userId, Guid deviceId)
    {
        var settings = await GetUserSettingsAsync(userId);
        var favorites = settings.FavoriteDevices?.Deserialize<List<Guid>>() ?? new List<Guid>();
        if (!favorites.Contains(deviceId))
        {
            favorites.Add(deviceId);
            settings.FavoriteDevices = JsonSerializer.SerializeToDocument(favorites);
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task<UserSettings> RemoveFavoriteDeviceAsync(Guid userId, Guid deviceId)
    {
        var settings = await GetUserSettingsAsync(userId);
        var favorites = settings.FavoriteDevices?.Deserialize<List<Guid>>() ?? new List<Guid>();
        if (favorites.Contains(deviceId))
        {
            favorites.Remove(deviceId);
            settings.FavoriteDevices = JsonSerializer.SerializeToDocument(favorites);
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task<UserSettings> UpdateRoomDisplayOrderAsync(Guid userId, object order)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.RoomDisplayOrder = JsonSerializer.SerializeToDocument(order);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateDeviceDisplayPreferencesAsync(Guid userId, object preferences)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.DeviceDisplayPreferences = JsonSerializer.SerializeToDocument(preferences);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateAutomationPreferencesAsync(Guid userId, object preferences)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.AutomationPreferences = JsonSerializer.SerializeToDocument(preferences);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }
} 