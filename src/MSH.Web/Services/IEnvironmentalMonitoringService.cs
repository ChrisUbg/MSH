using System.Threading.Tasks;

namespace MSH.Web.Services;

public interface IEnvironmentalMonitoringService
{
    Task ProcessIndoorTemperatureAsync(double temperature);
    Task ProcessOutdoorTemperatureAsync(double temperature);
    Task ProcessHumidityAsync(double humidity);
    Task ProcessAirQualityAsync(double co2, double voc);
    Task SetThresholdsAsync(EnvironmentalThresholds thresholds);
    Task<EnvironmentalThresholds> GetThresholdsAsync();
}

public class EnvironmentalThresholds
{
    public double IndoorTemperatureMin { get; set; } = 18.0; // °C
    public double IndoorTemperatureMax { get; set; } = 24.0; // °C
    public double OutdoorTemperatureMin { get; set; } = 0.0; // °C
    public double OutdoorTemperatureMax { get; set; } = 35.0; // °C
    public double HumidityMin { get; set; } = 30.0; // %
    public double HumidityMax { get; set; } = 60.0; // %
    public double CO2Max { get; set; } = 1000.0; // ppm
    public double VOCMax { get; set; } = 500.0; // ppb
} 