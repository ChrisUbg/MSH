namespace MSH.Infrastructure.Interfaces;

public interface IMatterDeviceControlService
{
    Task<bool> ToggleDeviceAsync(string nodeId);
    Task<bool> TurnOnDeviceAsync(string nodeId);
    Task<bool> TurnOffDeviceAsync(string nodeId);
    Task<string?> GetDeviceStateAsync(string nodeId);
    Task<bool> IsDeviceOnlineAsync(string nodeId);
    Task<PowerMetricsResult?> GetPowerMetricsAsync(string nodeId);
}

public class PowerMetricsResult
{
    public string? DeviceId { get; set; }
    public string? PowerState { get; set; }
    public decimal? PowerConsumption { get; set; }
    public decimal? Voltage { get; set; }
    public decimal? Current { get; set; }
    public decimal? EnergyToday { get; set; }
    public bool Online { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
