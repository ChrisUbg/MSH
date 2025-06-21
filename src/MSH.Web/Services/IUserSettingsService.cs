using System;
using System.Threading.Tasks;
using System.Text.Json;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface IUserSettingsService
{
    Task<UserSettings> GetUserSettingsAsync(string userId);
    Task<UserSettings> UpdateUserSettingsAsync(string userId, UserSettings settings);
    Task<UserSettings> UpdateThemeAsync(string userId, string theme);
    Task<UserSettings> UpdateLanguageAsync(string userId, string language);
    Task<UserSettings> UpdateNotificationPreferencesAsync(string userId, bool emailNotifications, bool pushNotifications);
    Task<UserSettings> UpdateDashboardLayoutAsync(string userId, JsonDocument layout);
    Task<UserSettings> AddFavoriteDeviceAsync(string userId, Guid deviceId);
    Task<UserSettings> RemoveFavoriteDeviceAsync(string userId, Guid deviceId);
    Task<UserSettings> UpdateRoomDisplayOrderAsync(string userId, object order);
    Task<UserSettings> UpdateDeviceDisplayPreferencesAsync(string userId, object preferences);
    Task<UserSettings> UpdateAutomationPreferencesAsync(string userId, object preferences);
} 