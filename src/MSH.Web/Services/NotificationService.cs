using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Services;
using MSH.Web.Hubs;

namespace MSH.Web.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ApplicationDbContext _dbContext;

    public NotificationService(IHubContext<NotificationHub> hubContext, ApplicationDbContext dbContext)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
    }

    // Implement INotificationService methods here, using _hubContext to send notifications

    public Task SendNotificationToUserAsync(int userId, string title, string message, string type)
    {
        // Example: send notification to a specific user group
        return _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", title, message, type);
    }

    public Task SendAlertToUserAsync(int userId, string title, string message, string alertType)
    {
        return _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveAlert", title, message, alertType);
    }

    public Task SendDeviceStatusUpdateAsync(int userId, int deviceId, string status)
    {
        return _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveDeviceStatusUpdate", deviceId, status);
    }

    public Task SendEnvironmentalAlertAsync(int userId, string parameter, double currentValue, double threshold)
    {
        return _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveEnvironmentalAlert", parameter, currentValue, threshold);
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId.ToString() && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkNotificationAsReadAsync(int notificationId)
    {
        var notification = await _dbContext.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
} 