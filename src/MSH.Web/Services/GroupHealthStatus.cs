using System;

namespace MSH.Web.Services
{
    public class GroupHealthStatus
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public string? Error { get; set; }
        public int TotalDevices { get; set; }
        public int OnlineDevices { get; set; }
        public DateTime LastUpdated { get; set; }
    }
} 