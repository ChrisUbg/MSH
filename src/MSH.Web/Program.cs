using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using MSH.Web.Services;
using MSH.Web.Hubs;
using MSH.Infrastructure.Services;
using Npgsql;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSH.Web.Components;
using MSH.Web.Components.Account;

var builder = WebApplication.CreateBuilder(args);

// Load port configuration
builder.Configuration.AddJsonFile("appsettings.Ports.json", optional: false);

// Configure DataProtection
builder.Services.AddDataProtection()
    .SetApplicationName("MSH")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")));

// Get port configuration
var webPort = builder.Configuration.GetValue<int>("Ports:Web:Internal");
var dbPort = builder.Configuration.GetValue<int>("Ports:Database:Internal");

// Environment-specific configuration
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);
    
    // Development database connection
    var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    var devConnectionString = envConnectionString ?? builder.Configuration.GetConnectionString("Development") 
        ?? $"Host=db;Port={dbPort};Database=matter_dev;Username=postgres;Password=devpassword";
    Console.WriteLine($"Using connection string: {devConnectionString}");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(devConnectionString, npgsqlOptions => 
            npgsqlOptions.MigrationsAssembly("MSH.Infrastructure")));
}
else
{
    // Production database connection
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions => 
            npgsqlOptions.MigrationsAssembly("MSH.Infrastructure")));
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
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEnvironmentalMonitoringService, EnvironmentalMonitoringService>();
builder.Services.AddSingleton<IBackupService, PostgresBackupService>();

// Add device simulator service
// builder.Services.AddSingleton<IDeviceSimulatorService, DeviceSimulatorService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

// Register hosted services
builder.Services.AddHostedService<BackupBackgroundService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5000") });

// Add services to the container.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

// Add Identity with UI
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDeviceTypeService, DeviceTypeService>();
builder.Services.AddScoped<IDeviceGroupService, DeviceGroupService>();
builder.Services.AddScoped<IUserLookupService, UserLookupService>();
builder.Services.AddScoped<MatterDeviceService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// app.MapRazorComponents<App>()
//     .AddInteractiveServerRenderMode();

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
    Console.WriteLine("Starting database migration...");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var maxRetries = 5;
    var retryDelay = TimeSpan.FromSeconds(5);

    for (int i = 0; i < maxRetries; i++)
    {
        try 
        {
            Console.WriteLine($"Migration attempt {i + 1} of {maxRetries}...");
            db.Database.Migrate();
            Console.WriteLine("✅ Database migration completed successfully");
            break;
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"❌ Migration attempt {i + 1} failed: {ex.Message}");
            if (i < maxRetries - 1)
            {
                Console.WriteLine($"Waiting {retryDelay.TotalSeconds} seconds before next attempt...");
                await Task.Delay(retryDelay);
            }
            else
            {
                Console.WriteLine("❌ All migration attempts failed");
                throw;
            }
        }
    }
    return;
}

// Add SignalR hub
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<DeviceHub>("/deviceHub");

app.MapRazorPages(); // For Identity UI
// app.MapRazorComponents<App>()
//     .AddInteractiveServerRenderMode();

app.Run();
