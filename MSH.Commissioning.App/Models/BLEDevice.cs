namespace MSH.Commissioning.App.Models
{
    public class BLEDevice
    {
        public string Address { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int RSSI { get; set; }
        public string Type { get; set; } = "BLE";
        public bool IsConnected { get; set; }
        public bool IsMatterDevice { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        
        public override string ToString()
        {
            return $"{Name} ({Address}) - {RSSI} dBm";
        }
    }

    public class CommissioningRequest
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceAddress { get; set; } = string.Empty;
        public string NetworkSSID { get; set; } = string.Empty;
        public string NetworkPassword { get; set; } = string.Empty;
        public string Passcode { get; set; } = string.Empty;
        public string Discriminator { get; set; } = string.Empty;
        public string NodeId { get; set; } = string.Empty;
        public string PiIP { get; set; } = "192.168.0.107";
        public string PiUser { get; set; } = "chregg";
    }

    public class CommissioningResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? NodeId { get; set; }
        public string? ErrorDetails { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class CommissioningProgress
    {
        public int Percentage { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
