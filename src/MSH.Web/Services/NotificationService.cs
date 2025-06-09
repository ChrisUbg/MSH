using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface INotificationService
{
    Task SendNotificationToUserAsync(Guid userId, string title, string message, string type);
    Task SendAlertToUserAsync(Guid userId, string title, string message, string severity);
    Task SendDeviceStatusUpdateAsync(Guid userId, Guid deviceId, string status);
    Task SendEnvironmentalAlertAsync(Guid userId, string parameter, double currentValue, double threshold);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
    Task MarkNotificationAsReadAsync(int notificationId);
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;

    public NotificationService(
        ILogger<NotificationService> logger,
        ApplicationDbContext dbContext,
        IEmailService emailService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task SendNotificationToUserAsync(Guid userId, string title, string message, string type)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = $"{title}: {message}",
                Type = Enum.Parse<NotificationType>(type, true),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Notification sent to user {UserId}: {Message}", userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            throw;
        }
    }

    public async Task SendAlertToUserAsync(Guid userId, string title, string message, string severity)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = $"[{severity}] {title}: {message}",
                Type = NotificationType.Warning,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            // Also send email for alerts
            await _emailService.SendEmailAsync(
                (await _dbContext.Users.FindAsync(userId))?.Email,
                $"[{severity}] {title}",
                message);

            _logger.LogInformation("Alert sent to user {UserId}: {Message}", userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert to user {UserId}", userId);
            throw;
        }
    }

    public async Task SendDeviceStatusUpdateAsync(Guid userId, Guid deviceId, string status)
    {
        try
        {
            var device = await _dbContext.Devices.FindAsync(deviceId);
            if (device == null)
            {
                throw new ArgumentException($"Device with ID {deviceId} not found");
            }

            var notification = new Notification
            {
                UserId = userId,
                Message = $"Device {device.Name} status updated: {status}",
                Type = NotificationType.Info,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Device status update sent to user {UserId} for device {DeviceId}", userId, deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending device status update to user {UserId}", userId);
            throw;
        }
    }

    public async Task SendEnvironmentalAlertAsync(Guid userId, string parameter, double currentValue, double threshold)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = $"Environmental alert: {parameter} is {currentValue} (threshold: {threshold})",
                Type = NotificationType.Warning,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Environmental alert sent to user {UserId} for {Parameter}", userId, parameter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending environmental alert to user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
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

public interface IEmailService
{
    Task SendEmailAsync(string? email, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string? email, string subject, string body)
    {
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Cannot send email: recipient email is null or empty");
            return;
        }

        // TODO: Implement actual email sending logic
        // This is a placeholder for the actual email sending implementation
        _logger.LogInformation("Would send email to {Email} with subject {Subject}", email, subject);
        await Task.CompletedTask;
    }
} 