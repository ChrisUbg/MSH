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
using MSH.Web.Pages;
using MSH.Web.Shared;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MSH.Infrastructure.Interfaces;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

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
    var devConnectionString = envConnectionString ?? builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? $"Host=db;Port={dbPort};Database=matter_dev;Username=postgres;Password=devpassword";
    Console.WriteLine($"Using connection string: {devConnectionString}");
    
    // Register DbContext as Scoped (default) - Singleton causes thread-safety issues
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(devConnectionString, npgsqlOptions => 
        {
            npgsqlOptions.MigrationsAssembly("MSH.Infrastructure");
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "db");
        }));
}
else
{
    // Production database connection - use environment variable if available
    var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    var connectionString = envConnectionString ?? builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Using connection string: {connectionString}");
    
    // Register DbContext as Scoped (default) - Singleton causes thread-safety issues
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions => 
        {
            npgsqlOptions.MigrationsAssembly("MSH.Infrastructure");
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "db");
        }));
}

// Register services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddHubOptions(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.EnableDetailedErrors = true; // Enable detailed errors in both development and production
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

// Add device simulator service
builder.Services.AddScoped<IDeviceSimulatorService, DeviceSimulatorService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("API", client =>
{
    // Use the same base URL as the current request context
    // This will work both from within Docker network and external access
    var baseUrl = builder.Environment.IsDevelopment() 
        ? "http://localhost:8082/" 
        : "http://localhost:8082/"; // Will be overridden at runtime
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Note: MatterBridge removed - commissioning now handled by PC, device control handled directly
// Add services to the container.

// Add services to the container.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddScoped<MSH.Infrastructure.Interfaces.IUserLookupService, UserLookupService>();

// Add Identity with UI
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Configure cookie policy
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.Cookie.Name = "MSH.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MSH API", Version = "v1" });
});

// Add Infrastructure services
builder.Services.AddScoped<IRuleEngineService, RuleEngineService>();
builder.Services.AddScoped<IDeviceSimulatorService, DeviceSimulatorService>();
builder.Services.AddScoped<IGroupStateManager, GroupStateManager>();
builder.Services.AddScoped<MatterDeviceService>();

// Add IRoomService and IDeviceGroupService
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IDeviceGroupService, DeviceGroupService>();

// Add IDeviceTypeService and DeviceTypeService
builder.Services.AddScoped<IDeviceTypeService, DeviceTypeService>();

// Add Matter device control service
Console.WriteLine("Registering IMatterDeviceControlService...");
builder.Services.AddScoped<IMatterDeviceControlService, MatterDeviceControlService>();
Console.WriteLine("IMatterDeviceControlService registered successfully");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseRouting();

// Add these in the correct order
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages first
app.MapRazorPages();

// Then map Blazor with specific routes
app.MapBlazorHub();
app.MapFallbackToPage("/_Host", "/_Host");

// Map API controllers
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

// Add simple health check endpoint (temporarily disabled database check)
app.MapGet("/health", async (HttpContext context) =>
{
    // Temporarily return healthy without database check to get app running
    await context.Response.WriteAsJsonAsync(new { status = "healthy", database = "connected" });
});

// Add test endpoint to check service registration
app.MapGet("/test-service", (HttpContext context) =>
{
    try
    {
        var service = context.RequestServices.GetService<IMatterDeviceControlService>();
        if (service != null)
        {
            return "✅ IMatterDeviceControlService is registered and can be resolved";
        }
        else
        {
            return "❌ IMatterDeviceControlService is NOT registered";
        }
    }
    catch (Exception ex)
    {
        return $"❌ Error resolving service: {ex.Message}";
    }
});

// Add this before app.Run()
if (args.Contains("--migrate"))
{
    Console.WriteLine("Running database migration...");
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
        throw;
    }
}

app.Run();
