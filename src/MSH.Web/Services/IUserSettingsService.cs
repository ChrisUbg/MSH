using System;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface IUserSettingsService
{
    Task<UserSettings> GetUserSettingsAsync(Guid userId);
    Task<UserSettings> UpdateUserSettingsAsync(Guid userId, UserSettings settings);
    Task<UserSettings> UpdateThemeAsync(Guid userId, string theme);
    Task<UserSettings> UpdateLanguageAsync(Guid userId, string language);
    Task<UserSettings> UpdateDashboardLayoutAsync(Guid userId, object layout);
    Task<UserSettings> UpdateNotificationPreferencesAsync(Guid userId, object preferences);
    Task<UserSettings> AddFavoriteDeviceAsync(Guid userId, Guid deviceId);
    Task<UserSettings> RemoveFavoriteDeviceAsync(Guid userId, Guid deviceId);
    Task<UserSettings> UpdateRoomDisplayOrderAsync(Guid userId, object order);
    Task<UserSettings> UpdateDeviceDisplayPreferencesAsync(Guid userId, object preferences);
    Task<UserSettings> UpdateAutomationPreferencesAsync(Guid userId, object preferences);
} 