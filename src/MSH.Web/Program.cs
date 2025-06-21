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
    var devConnectionString = envConnectionString ?? builder.Configuration.GetConnectionString("Development") 
        ?? $"Host=db;Port={dbPort};Database=matter_dev;Username=postgres;Password=devpassword";
    Console.WriteLine($"Using connection string: {devConnectionString}");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(devConnectionString, npgsqlOptions => 
            npgsqlOptions.MigrationsAssembly("MSH.Infrastructure")));
}
else
{
    // Production database connection - use environment variable if available
    var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    var connectionString = envConnectionString ?? builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Using connection string: {connectionString}");
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

// Add device simulator service
builder.Services.AddScoped<IDeviceSimulatorService, DeviceSimulatorService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("http://localhost:8082/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Add Matter bridge HTTP client
builder.Services.AddHttpClient("MatterBridge", client =>
{
    var matterBridgeUrl = builder.Configuration["MatterBridge:BaseUrl"] ?? "http://matter-bridge:8084";
    client.BaseAddress = new Uri(matterBridgeUrl);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

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

// Map API controllers
app.MapControllers();

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

// Add simple health check endpoint
app.MapGet("/health", async (HttpContext context, ApplicationDbContext db) =>
{
    try
    {
        // Try to connect to the database
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect)
        {
            await context.Response.WriteAsJsonAsync(new { status = "healthy", database = "connected" });
            return;
        }
        context.Response.StatusCode = 503;
        await context.Response.WriteAsJsonAsync(new { status = "unhealthy", database = "disconnected" });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 503;
        await context.Response.WriteAsJsonAsync(new { status = "unhealthy", error = ex.Message });
    }
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
        }
    }
}

app.Run();
