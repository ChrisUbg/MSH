using Microsoft.EntityFrameworkCore;
using MSH.Infrastructure.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Environment-specific configuration
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);
    
    // Development database connection
    var devConnectionString = builder.Configuration.GetConnectionString("Development") 
        ?? "Host=localhost;Port=5432;Database=matter_dev;Username=postgres;Password=devpassword";
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(devConnectionString));
}
else
{
    try 
    {
        Console.WriteLine("Initializing production database connection...");
        
        var password = File.ReadAllText("/run/secrets/postgres_password").Trim();
        var connectionString = $"Host=db;Port=5432;Database=matter_prod;Username=postgres;Password={password};Pooling=true;Timeout=30";
        
        // Test connection without psql
        Console.WriteLine("Testing raw TCP connection to db:5432...");
        using (var socket = new System.Net.Sockets.TcpClient())
        {
            socket.ConnectAsync("db", 5432).Wait();
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

        // Verify EF Core can connect
        using var scope = builder.Services.BuildServiceProvider().CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        db.Database.OpenConnection();
        Console.WriteLine("✅ Database connection successful");
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

// Ensure this comes AFTER AddServerSideBlazor()
builder.Services.AddSignalR();

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("/network-diag", () => {
    var sb = new StringBuilder();
    try {
        sb.AppendLine("DB Connection Test:");
        using var socket = new System.Net.Sockets.TcpClient();
        socket.Connect("db", 5432);
        sb.AppendLine("✅ Connected to db:5432");
    } catch (Exception ex) {
        sb.AppendLine($"❌ Connection failed: {ex.Message}");
    }
    return sb.ToString();
});

// Add this before app.Run()
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
    return;
}

app.Run();
