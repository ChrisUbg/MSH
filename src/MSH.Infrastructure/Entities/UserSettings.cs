using System;
using System.Text.Json;

namespace MSH.Infrastructure.Entities;

public class UserSettings : BaseEntity
{
    public Guid UserId { get; set; }
    
    // UI Preferences
    public string Theme { get; set; } = "light"; // light, dark, system
    public string Language { get; set; } = "en"; // language code
    public bool ShowOfflineDevices { get; set; } = true;
    public string DefaultView { get; set; } = "rooms"; // rooms, devices, groups
    
    // Dashboard Preferences
    public JsonDocument? DashboardLayout { get; set; } // Custom dashboard layout
    public JsonDocument? FavoriteDevices { get; set; } // List of favorite device IDs
    public JsonDocument? QuickActions { get; set; } // Custom quick actions
    
    // Notification Settings
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public JsonDocument? NotificationPreferences { get; set; } // Detailed notification settings
    
    // Device Preferences
    public JsonDocument? DeviceDisplayPreferences { get; set; } // How devices are displayed
    public JsonDocument? LastUsedDevices { get; set; } // Recently used devices
    
    // Room Preferences
    public JsonDocument? RoomDisplayOrder { get; set; } // Custom room ordering
    public bool ShowEmptyRooms { get; set; } = true;
    
    // Automation Preferences
    public bool ShowAutomationSuggestions { get; set; } = true;
    public JsonDocument? AutomationPreferences { get; set; } // Automation-related settings
    
    // Navigation properties
    public User User { get; set; } = null!;
} 