namespace MSH.Web.Services;

public static class LoggingConfig
{
    // Logging switches - can be controlled via configuration
    public static bool EnableDeviceControlLogs { get; set; } = true;
    public static bool EnableEventDelayLogs { get; set; } = true;
    public static bool EnableUILogs { get; set; } = true;
    public static bool EnableServiceLogs { get; set; } = true;
    public static bool EnableDebugLogs { get; set; } = true;
    
    // Helper methods for conditional logging
    public static void LogDeviceControl(string message)
    {
        if (EnableDeviceControlLogs)
            Console.WriteLine($"[DEVICE_CONTROL] {message}");
    }
    
    public static void LogEventDelay(string message)
    {
        if (EnableEventDelayLogs)
            Console.WriteLine($"[EVENT_DELAY] {message}");
    }
    
    public static void LogUI(string message)
    {
        if (EnableUILogs)
            Console.WriteLine($"[UI] {message}");
    }
    
    public static void LogService(string message)
    {
        if (EnableServiceLogs)
            Console.WriteLine($"[SERVICE] {message}");
    }
    
    public static void LogDebug(string message)
    {
        if (EnableDebugLogs)
            Console.WriteLine($"[DEBUG] {message}");
    }
}
