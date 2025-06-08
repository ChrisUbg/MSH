using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Services;

namespace MSH.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly IUserLookupService? _userLookupService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IUserLookupService? userLookupService = null) : base(options)
    {
        _userLookupService = userLookupService;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSettings> UserSettings { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<DeviceType> DeviceTypes { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<DeviceGroup> DeviceGroups { get; set; } = null!;
    public DbSet<DeviceGroupMember> DeviceGroupMembers { get; set; } = null!;
    public DbSet<DeviceState> DeviceStates { get; set; } = null!;
    public DbSet<DeviceEvent> DeviceEvents { get; set; } = null!;
    public DbSet<Rule> Rules { get; set; } = null!;
    public DbSet<RuleTrigger> RuleTriggers { get; set; } = null!;
    public DbSet<UserDevicePermission> UserDevicePermissions { get; set; } = null!;
    public DbSet<UserRoomPermission> UserRoomPermissions { get; set; } = null!;
    public DbSet<EnvironmentalSettings> EnvironmentalSettings { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var lambda = Expression.Lambda(Expression.Equal(property, falseConstant), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);

                // Configure user tracking relationships
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(User), nameof(BaseEntity.CreatedBy))
                    .WithMany()
                    .HasForeignKey(nameof(BaseEntity.CreatedById))
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(User), nameof(BaseEntity.UpdatedBy))
                    .WithMany()
                    .HasForeignKey(nameof(BaseEntity.UpdatedById))
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }

        // Configure unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.MatterDeviceId)
            .IsUnique()
            .HasFilter("\"MatterDeviceId\" IS NOT NULL");

        // Configure UserSettings
        modelBuilder.Entity<UserSettings>()
            .HasOne(us => us.User)
            .WithOne()
            .HasForeignKey<UserSettings>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationships
        modelBuilder.Entity<Device>()
            .HasOne(d => d.DeviceType)
            .WithMany(dt => dt.Devices)
            .HasForeignKey(d => d.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Device>()
            .HasOne(d => d.Room)
            .WithMany(r => r.Devices)
            .HasForeignKey(d => d.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EnvironmentalSettings>(entity =>
        {
            entity.HasOne<User>()
                .WithOne()
                .HasForeignKey<EnvironmentalSettings>(es => es.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Add more configurations as needed
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var currentUserId = _userLookupService?.GetCurrentUserId();

        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                if (currentUserId.HasValue)
                {
                    entry.Entity.CreatedById = currentUserId.Value;
                }
            }
            else
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                if (currentUserId.HasValue)
                {
                    entry.Entity.UpdatedById = currentUserId.Value;
                }
            }
        }
    }
} 