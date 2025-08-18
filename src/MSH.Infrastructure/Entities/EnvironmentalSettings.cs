using System.ComponentModel.DataAnnotations;

namespace MSH.Infrastructure.Entities;

public class EnvironmentalSettings : BaseEntity
{
    [MaxLength(150)]
    public string? UserId { get; set; }
    
    public double IndoorTemperatureMin { get; set; } = 18.0;
    
    public double IndoorTemperatureMax { get; set; } = 24.0;
    
    public double OutdoorTemperatureMin { get; set; } = 0.0;
    
    public double OutdoorTemperatureMax { get; set; } = 35.0;
    
    public double HumidityMin { get; set; } = 30.0;
    
    public double HumidityMax { get; set; } = 60.0;
    
    public double CO2Max { get; set; } = 1000.0;
    
    public double VOCMax { get; set; } = 500.0;
    
    public double TemperatureWarning { get; set; } = 15.0;
    
    public double HumidityWarning { get; set; } = 40.0;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
} 