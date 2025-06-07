using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly ApplicationDbContext _context;

    public UserSettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings> GetUserSettingsAsync(int userId)
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

    public async Task<UserSettings> UpdateUserSettingsAsync(int userId, UserSettings settings)
    {
        var existingSettings = await GetUserSettingsAsync(userId);
        
        // Update all properties
        _context.Entry(existingSettings).CurrentValues.SetValues(settings);
        existingSettings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existingSettings;
    }

    public async Task<UserSettings> UpdateThemeAsync(int userId, string theme)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.Theme = theme;
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateLanguageAsync(int userId, string language)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.Language = language;
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateDashboardLayoutAsync(int userId, object layout)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.DashboardLayout = JsonSerializer.SerializeToDocument(layout);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateNotificationPreferencesAsync(int userId, object preferences)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.NotificationPreferences = JsonSerializer.SerializeToDocument(preferences);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> AddFavoriteDeviceAsync(int userId, int deviceId)
    {
        var settings = await GetUserSettingsAsync(userId);
        var favorites = settings.FavoriteDevices != null 
            ? JsonSerializer.Deserialize<int[]>(settings.FavoriteDevices)?.ToList() ?? new List<int>()
            : new List<int>();

        if (!favorites.Contains(deviceId))
        {
            favorites.Add(deviceId);
            settings.FavoriteDevices = JsonSerializer.SerializeToDocument(favorites);
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<UserSettings> RemoveFavoriteDeviceAsync(int userId, int deviceId)
    {
        var settings = await GetUserSettingsAsync(userId);
        if (settings.FavoriteDevices != null)
        {
            var favorites = JsonSerializer.Deserialize<int[]>(settings.FavoriteDevices)?.ToList() ?? new List<int>();
            favorites.Remove(deviceId);
            settings.FavoriteDevices = JsonSerializer.SerializeToDocument(favorites);
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<UserSettings> UpdateRoomDisplayOrderAsync(int userId, object order)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.RoomDisplayOrder = JsonSerializer.SerializeToDocument(order);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateDeviceDisplayPreferencesAsync(int userId, object preferences)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.DeviceDisplayPreferences = JsonSerializer.SerializeToDocument(preferences);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateAutomationPreferencesAsync(int userId, object preferences)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.AutomationPreferences = JsonSerializer.SerializeToDocument(preferences);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }
} 