using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace MSH.Infrastructure.Services;

public class MatterDeviceService
{
    private readonly string _matterSdkPath;
    private readonly string _chipToolPath;

    public MatterDeviceService(IConfiguration config)
    {
        _matterSdkPath = config["Matter:SdkPath"] ?? "/matter";
        _chipToolPath = config["Matter:ChipToolPath"] ?? "/usr/local/bin/chip-tool";

        // In Docker, we don't need to check for the SDK path as it's mounted
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            if (!Directory.Exists(_matterSdkPath))
                throw new DirectoryNotFoundException($"Matter SDK path not found: {_matterSdkPath}");
            if (!File.Exists(_chipToolPath))
                throw new FileNotFoundException($"chip-tool not found: {_chipToolPath}");
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

    private void RunMatterCommand(string command)
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
        process.WaitForExit();

        // 4. Fehlerbehandlung
        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new Exception($"Matter-Befehl fehlgeschlagen: {error}");
        }
    }
}
