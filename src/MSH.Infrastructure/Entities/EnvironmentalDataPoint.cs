namespace MSH.Infrastructure.Entities;

public class EnvironmentalDataPoint
{
    public DateTime Timestamp { get; set; }
    public double IndoorTemperature { get; set; }
    
    public double OutdoorTemperature { get; set; }
    public double Humidity { get; set; }
    public double CO2 { get; set; }
    public double VOC { get; set; }
} 