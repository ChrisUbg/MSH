using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MSH.Web.Data;

namespace MSH.Web.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        var connectionString = envConnectionString ?? config.GetConnectionString("Development");
        Console.WriteLine($"[Factory] Raw connection string: {connectionString}");
        Console.WriteLine($"[Factory] Current directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"[Factory] Config files found: {string.Join(", ", Directory.GetFiles(Directory.GetCurrentDirectory(), "appsettings*.json"))}");

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseNpgsql(connectionString);

        return new ApplicationDbContext(builder.Options);
    }
} 