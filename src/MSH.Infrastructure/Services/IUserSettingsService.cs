using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public interface IUserSettingsService
{
    Task<UserSettings> GetUserSettingsAsync(int userId);
    Task<UserSettings> UpdateUserSettingsAsync(int userId, UserSettings settings);
    Task<UserSettings> UpdateThemeAsync(int userId, string theme);
    Task<UserSettings> UpdateLanguageAsync(int userId, string language);
    Task<UserSettings> UpdateDashboardLayoutAsync(int userId, object layout);
    Task<UserSettings> UpdateNotificationPreferencesAsync(int userId, object preferences);
    Task<UserSettings> AddFavoriteDeviceAsync(int userId, int deviceId);
    Task<UserSettings> RemoveFavoriteDeviceAsync(int userId, int deviceId);
    Task<UserSettings> UpdateRoomDisplayOrderAsync(int userId, object order);
    Task<UserSettings> UpdateDeviceDisplayPreferencesAsync(int userId, object preferences);
    Task<UserSettings> UpdateAutomationPreferencesAsync(int userId, object preferences);
} 