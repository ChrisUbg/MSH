using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Infrastructure.Services;
using MSH.Web.Hubs;
using MSH.Web.Services;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load port configuration
builder.Configuration.AddJsonFile("appsettings.Ports.json", optional: false);

// Configure DataProtection
builder.Services.AddDataProtection()
    .SetApplicationName("MSH")
    .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"));

// Get port configuration
var webPort = builder.Configuration.GetValue<int>("Ports:Web:Internal");
var dbPort = builder.Configuration.GetValue<int>("Ports:Database:Internal");

// Environment-specific configuration
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);
    
    // Development database connection
    var devConnectionString = builder.Configuration.GetConnectionString("Development") 
        ?? $"Host=db;Port={dbPort};Database=matter_dev;Username=postgres;Password=devpassword";
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(devConnectionString));
}
else
{
    try 
    {
        Console.WriteLine("Initializing production database connection...");
        
        var password = File.ReadAllText("/run/secrets/postgres_password").Trim();
        var connectionString = $"Host=db;Port={dbPort};Database=matter_prod;Username=postgres;Password={password};Pooling=true;Timeout=30;Include Error Detail=true";
        
        // Test connection without psql
        Console.WriteLine($"Testing raw TCP connection to db:{dbPort}...");
        using (var socket = new System.Net.Sockets.TcpClient())
        {
            socket.ConnectAsync("db", dbPort).Wait();
            Console.WriteLine("✅ TCP connection successful");
        }

        builder.Services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(connectionString, npgsqlOptions => 
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
            }));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Connection failed: {ex}");
        throw;
    }
}

// Register services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddHubOptions(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Add controllers
builder.Services.AddControllers();

// Ensure this comes AFTER AddServerSideBlazor()
builder.Services.AddSignalR();

// Add HttpContextAccessor for CurrentUserService
builder.Services.AddHttpContextAccessor();

// Add environmental monitoring services
builder.Services.AddScoped<MSH.Infrastructure.Services.IUserLookupService, MSH.Infrastructure.Services.UserLookupService>();
builder.Services.AddScoped<MSH.Infrastructure.Services.ICurrentUserService, MSH.Web.Services.CurrentUserService>();
builder.Services.AddScoped<MSH.Infrastructure.Services.INotificationService, MSH.Web.Services.NotificationService>();
builder.Services.AddScoped<MSH.Infrastructure.Services.IEmailService, MSH.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<MSH.Infrastructure.Services.IEnvironmentalMonitoringService, MSH.Infrastructure.Services.EnvironmentalMonitoringService>();
builder.Services.AddSingleton<MSH.Infrastructure.Services.IBackupService, MSH.Infrastructure.Services.PostgresBackupService>();

// Register hosted services
builder.Services.AddHostedService<MSH.Web.Services.BackupBackgroundService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5000") });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Add basic middleware
app.Use(async (context, next) =>
{
    try
    {
        Console.WriteLine($"Processing request: {context.Request.Path}");
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing request: {ex}");
        throw;
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add this line to enable controllers
app.MapControllers();

// Configure URLs explicitly
app.Urls.Clear();
var port = builder.Configuration.GetValue<int>("Ports:Web:Internal");
Console.WriteLine($"Configuring application to listen on port {port}");
app.Urls.Add($"http://0.0.0.0:{port}");

// Add basic endpoint for testing
app.MapGet("/test", () => {
    Console.WriteLine("Test endpoint called");
    return "Hello World!";
});

// Configure Blazor
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("/network-diag", () => {
    var sb = new StringBuilder();
    try {
        sb.AppendLine("DB Connection Test:");
        using var socket = new System.Net.Sockets.TcpClient();
        socket.Connect("db", dbPort);
        sb.AppendLine($"✅ Connected to db:{dbPort}");
    } catch (Exception ex) {
        sb.AppendLine($"❌ Connection failed: {ex.Message}");
    }
    return sb.ToString();
});

// Add this before app.Run()
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    for (int i = 0; i < 5; i++) // Retry loop
    {
        try {
            db.Database.Migrate();
            break;
        } catch (NpgsqlException) {
            await Task.Delay(5000);
        }
    }
    return;
}

// Add SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
