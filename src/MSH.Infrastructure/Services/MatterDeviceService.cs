using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;

namespace MSH.Infrastructure.Services;

public class MatterDeviceService
{
    private readonly string _matterSdkPath;
    private readonly string _chipToolPath;

    public MatterDeviceService(IConfiguration config)
    {
        // Get configuration with fallbacks
        _matterSdkPath = Environment.GetEnvironmentVariable("MATTER_SDK_PATH") ?? 
                        config["Matter:SdkPath"] ?? 
                        "/matter";
        _chipToolPath = Environment.GetEnvironmentVariable("CHIP_TOOL_PATH") ?? 
                       config["Matter:ChipToolPath"] ?? 
                       "/usr/local/bin/chip-tool";

        // Only validate paths in development and if not running in container
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true" && 
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            // Log warnings instead of throwing exceptions
            if (!Directory.Exists(_matterSdkPath))
            {
                Console.WriteLine($"Warning: Matter SDK path not found: {_matterSdkPath}");
            }
            if (!File.Exists(_chipToolPath))
            {
                Console.WriteLine($"Warning: chip-tool not found: {_chipToolPath}");
            }
        }

        Pin = "20202021"; // Default value
    }
    public string Pin { get; private set; }

    public void SetPin(string pin)
    {
        Pin = pin;
    }
    public void TurnOn(string deviceId) => RunMatterCommand($"onoff on {deviceId} 1");
    public void TurnOff(string deviceId) => RunMatterCommand($"onoff off {deviceId} 1");

    public async Task<Dictionary<string, object>> GetPowerMetrics(string deviceId)
    {
        try
        {
            // Get power measurement
            var powerCmd = $"electricalmeasurement read measurement-type {deviceId} 1";
            var powerResult = RunMatterCommand(powerCmd);
            
            // Get energy measurement
            var energyCmd = $"electricalmeasurement read total-energy {deviceId} 1";
            var energyResult = RunMatterCommand(energyCmd);

            var metrics = new Dictionary<string, object>();
            
            // Parse power measurement
            foreach (var line in powerResult.Split('\n'))
            {
                if (line.Contains("measurement-type:"))
                {
                    var value = line.Split(':')[1].Trim();
                    metrics["power"] = double.Parse(value);
                }
            }

            // Parse energy measurement
            foreach (var line in energyResult.Split('\n'))
            {
                if (line.Contains("total-energy:"))
                {
                    var value = line.Split(':')[1].Trim();
                    metrics["energy"] = double.Parse(value);
                }
            }

            return metrics;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get power metrics: {ex.Message}");
        }
    }

    private string RunMatterCommand(string command)
    {
        // 1. Environment vorbereiten
        var envVars = new Dictionary<string, string>
        {
            ["PATH"] = "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin",
            ["MATTER_ROOT"] = _matterSdkPath
        };

        // 2. Prozess starten
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c 'source {_matterSdkPath}/scripts/activate.sh && {_chipToolPath} {command}'",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = _matterSdkPath
            }
        };

        // 3. Umgebungsvariablen setzen
        foreach (var envVar in envVars)
        {
            process.StartInfo.Environment[envVar.Key] = envVar.Value;
        }

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        // 4. Fehlerbehandlung
        if (process.ExitCode != 0)
        {
            throw new Exception($"Matter-Befehl fehlgeschlagen: {error}");
        }

        return output;
    }
}
