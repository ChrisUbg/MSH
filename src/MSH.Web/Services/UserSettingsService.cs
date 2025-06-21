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

    public async Task<UserSettings> GetUserSettingsAsync(string userId)
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

    public async Task<UserSettings> UpdateUserSettingsAsync(string userId, UserSettings settings)
    {
        var existingSettings = await GetUserSettingsAsync(userId);
        
        // Update all properties
        _context.Entry(existingSettings).CurrentValues.SetValues(settings);
        existingSettings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existingSettings;
    }

    public async Task<UserSettings> UpdateThemeAsync(string userId, string theme)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.Theme = theme;
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateLanguageAsync(string userId, string language)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.Language = language;
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateNotificationPreferencesAsync(string userId, bool emailNotifications, bool pushNotifications)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.EmailNotifications = emailNotifications;
        settings.PushNotifications = pushNotifications;
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateDashboardLayoutAsync(string userId, JsonDocument layout)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.DashboardLayout = layout;
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> AddFavoriteDeviceAsync(string userId, Guid deviceId)
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

    public async Task<UserSettings> RemoveFavoriteDeviceAsync(string userId, Guid deviceId)
    {
        var settings = await GetUserSettingsAsync(userId);
        
        var favorites = settings.FavoriteDevices?.Deserialize<List<Guid>>() ?? new List<Guid>();
        favorites.Remove(deviceId);
        settings.FavoriteDevices = JsonSerializer.SerializeToDocument(favorites);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateRoomDisplayOrderAsync(string userId, object order)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.RoomDisplayOrder = JsonSerializer.SerializeToDocument(order);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateDeviceDisplayPreferencesAsync(string userId, object preferences)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.DeviceDisplayPreferences = JsonSerializer.SerializeToDocument(preferences);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> UpdateAutomationPreferencesAsync(string userId, object preferences)
    {
        var settings = await GetUserSettingsAsync(userId);
        settings.AutomationPreferences = JsonSerializer.SerializeToDocument(preferences);
        settings.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return settings;
    }
} 