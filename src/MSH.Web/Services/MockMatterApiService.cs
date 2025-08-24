using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MSH.Web.Services
{
    public class MockMatterApiService : BackgroundService
    {
        private readonly ILogger<MockMatterApiService> _logger;
        private readonly WebApplication _app;

        public MockMatterApiService(ILogger<MockMatterApiService> logger)
        {
            _logger = logger;
            
            // Create a minimal web app for the mock API
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddControllers();
            
            // Configure the URL
            builder.WebHost.UseUrls("http://localhost:8000");
            
            _app = builder.Build();
            
            // Configure mock endpoints
            ConfigureMockEndpoints();
        }

        private void ConfigureMockEndpoints()
        {
            // Mock device toggle endpoint
            _app.MapPost("/api/matter/device/{nodeId}/toggle", (string nodeId) =>
            {
                _logger.LogInformation("Mock API: Toggle device {NodeId}", nodeId);
                
                // Simulate some processing time (but much faster than Docker)
                Thread.Sleep(200);
                
                // Simulate success/failure randomly (90% success rate)
                var success = Random.Shared.Next(1, 11) <= 9;
                
                var response = new
                {
                    Success = success,
                    Message = success ? "Device toggled successfully" : "Device not reachable",
                    NewState = success ? (Random.Shared.Next(2) == 0 ? "on" : "off") : null
                };
                
                return Results.Json(response);
            });

            // Mock device state endpoint
            _app.MapGet("/api/matter/device/{nodeId}/state", (string nodeId) =>
            {
                _logger.LogInformation("Mock API: Get state for device {NodeId}", nodeId);
                
                // Simulate some processing time
                Thread.Sleep(100);
                
                var response = new
                {
                    State = Random.Shared.Next(2) == 0 ? "on" : "off",
                    Success = true,
                    Message = "State retrieved successfully"
                };
                
                return Results.Json(response);
            });

            // Mock device online check endpoint
            _app.MapGet("/api/matter/device/{nodeId}/online", (string nodeId) =>
            {
                _logger.LogInformation("Mock API: Check online status for device {NodeId}", nodeId);
                
                // Simulate some processing time
                Thread.Sleep(50);
                
                var response = new
                {
                    IsOnline = Random.Shared.Next(1, 11) <= 8, // 80% online rate
                    Success = true,
                    Message = "Online status checked"
                };
                
                return Results.Json(response);
            });

            // Mock power metrics endpoint
            _app.MapGet("/api/matter/device/{nodeId}/power-metrics", (string nodeId) =>
            {
                _logger.LogInformation("Mock API: Get power metrics for device {NodeId}", nodeId);
                
                // Simulate some processing time
                Thread.Sleep(150);
                
                // Simulate realistic power consumption data
                var isOn = Random.Shared.Next(2) == 0;
                var powerConsumption = isOn ? Random.Shared.Next(5, 150) + Random.Shared.NextDouble() : 0;
                var voltage = 230.0 + (Random.Shared.NextDouble() - 0.5) * 20; // 220-240V range
                var current = powerConsumption / voltage;
                var energyToday = Random.Shared.Next(1, 10) + Random.Shared.NextDouble();
                
                var response = new
                {
                    DeviceId = nodeId,
                    PowerState = isOn ? "on" : "off",
                    PowerConsumption = Math.Round(powerConsumption, 1),
                    Voltage = Math.Round(voltage, 1),
                    Current = Math.Round(current, 3),
                    EnergyToday = Math.Round(energyToday, 2),
                    Online = Random.Shared.Next(1, 11) <= 9, // 90% online rate
                    Success = true,
                    Message = "Power metrics retrieved successfully"
                };
                
                return Results.Json(response);
            });

            // Health check endpoint
            _app.MapGet("/api/matter/health", () =>
            {
                return Results.Json(new { Status = "Mock API Running", Timestamp = DateTime.UtcNow });
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting Mock Matter API on port 8000");
                await _app.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Mock Matter API");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _app.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
