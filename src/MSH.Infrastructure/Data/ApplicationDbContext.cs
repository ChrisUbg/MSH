using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using MSH.Infrastructure.Interfaces;

namespace MSH.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly IUserLookupService? _userLookupService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IUserLookupService? userLookupService = null) : base(options)
    {
        _userLookupService = userLookupService;
    }

    public new DbSet<IdentityUser> Users { get; set; } = null!;
    public DbSet<User> ApplicationUsers { get; set; } = null!;
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
    public DbSet<DeviceHistory> DeviceHistory { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<GroupMember> GroupMembers { get; set; } = null!;
    public DbSet<GroupState> GroupStates { get; set; } = null!;
    public DbSet<GroupStateHistory> GroupStateHistory { get; set; } = null!;
    public DbSet<RuleCondition> RuleConditions { get; set; } = null!;
    public DbSet<RuleAction> RuleActions { get; set; } = null!;
    public DbSet<RuleExecutionHistory> RuleExecutionHistory { get; set; } = null!;

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
            .HasIndex(u => u.UserName)
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
            .HasKey(us => us.UserId);

        modelBuilder.Entity<UserSettings>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<UserSettings>(us => us.UserId);

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

        modelBuilder.Entity<DeviceGroupMember>()
            .HasKey(dgm => new { dgm.DeviceId, dgm.DeviceGroupId });

        modelBuilder.Entity<DeviceGroupMember>()
            .HasOne(dgm => dgm.Device)
            .WithMany(d => d.DeviceGroupMembers)
            .HasForeignKey(dgm => dgm.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceGroupMember>()
            .HasOne(dgm => dgm.DeviceGroup)
            .WithMany(dg => dg.DeviceGroupMembers)
            .HasForeignKey(dgm => dgm.DeviceGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EnvironmentalSettings>(entity =>
        {
            entity.HasOne<User>()
                .WithOne()
                .HasForeignKey<EnvironmentalSettings>(es => es.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Device
        modelBuilder.Entity<Device>()
            .HasKey(d => d.Id);

        modelBuilder.Entity<Device>()
            .Property(d => d.Properties)
            .HasColumnType("jsonb");

        // Configure DeviceHistory
        modelBuilder.Entity<DeviceHistory>()
            .HasKey(dh => dh.Id);

        modelBuilder.Entity<DeviceHistory>()
            .HasOne<Device>()
            .WithMany()
            .HasForeignKey(dh => dh.DeviceId);

        // Configure Group
        modelBuilder.Entity<Group>()
            .HasKey(g => g.Id);

        // Configure GroupMember
        modelBuilder.Entity<GroupMember>()
            .HasKey(gm => new { gm.GroupId, gm.DeviceId });

        modelBuilder.Entity<GroupMember>()
            .HasOne<Group>()
            .WithMany()
            .HasForeignKey(gm => gm.GroupId);

        modelBuilder.Entity<GroupMember>()
            .HasOne<Device>()
            .WithMany()
            .HasForeignKey(gm => gm.DeviceId);

        // Configure GroupState
        modelBuilder.Entity<GroupState>()
            .HasKey(gs => gs.GroupId);

        modelBuilder.Entity<GroupState>()
            .HasOne(gs => gs.Group)
            .WithOne(g => g.State)
            .HasForeignKey<GroupState>(gs => gs.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GroupState>()
            .Property(gs => gs.State)
            .HasColumnType("jsonb");

        // Configure GroupStateHistory
        modelBuilder.Entity<GroupStateHistory>()
            .HasKey(gsh => gsh.Id);

        modelBuilder.Entity<GroupStateHistory>()
            .HasOne<Group>()
            .WithMany()
            .HasForeignKey(gsh => gsh.GroupId);

        modelBuilder.Entity<GroupStateHistory>()
            .Property(gsh => gsh.OldState)
            .HasColumnType("jsonb");

        modelBuilder.Entity<GroupStateHistory>()
            .Property(gsh => gsh.NewState)
            .HasColumnType("jsonb");

        // Configure Rule
        modelBuilder.Entity<Rule>()
            .HasKey(r => r.Id);

        // Configure RuleCondition
        modelBuilder.Entity<RuleCondition>()
            .HasKey(rc => rc.Id);

        modelBuilder.Entity<RuleCondition>()
            .HasOne<Rule>()
            .WithMany()
            .HasForeignKey(rc => rc.RuleId);

        modelBuilder.Entity<RuleCondition>()
            .Property(rc => rc.Condition)
            .HasColumnType("jsonb");

        // Configure RuleAction
        modelBuilder.Entity<RuleAction>()
            .HasKey(ra => ra.Id);

        modelBuilder.Entity<RuleAction>()
            .HasOne<Rule>()
            .WithMany()
            .HasForeignKey(ra => ra.RuleId);

        modelBuilder.Entity<RuleAction>()
            .Property(ra => ra.Action)
            .HasColumnType("jsonb");

        // Configure RuleTrigger
        modelBuilder.Entity<RuleTrigger>()
            .HasKey(rt => rt.Id);

        modelBuilder.Entity<RuleTrigger>()
            .HasOne<Rule>()
            .WithMany()
            .HasForeignKey(rt => rt.RuleId);

        modelBuilder.Entity<RuleTrigger>()
            .Property(rt => rt.Trigger)
            .HasColumnType("jsonb");

        // Configure RuleExecutionHistory
        modelBuilder.Entity<RuleExecutionHistory>()
            .HasKey(reh => reh.Id);

        modelBuilder.Entity<RuleExecutionHistory>()
            .HasOne<Rule>()
            .WithMany()
            .HasForeignKey(reh => reh.RuleId);

        modelBuilder.Entity<RuleExecutionHistory>()
            .Property(reh => reh.Result)
            .HasColumnType("jsonb");
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
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added
                || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                if (_userLookupService != null)
                {
                    var userId = _userLookupService.GetCurrentUserId();
                    if (userId.HasValue)
                    {
                        entity.CreatedById = (Guid)userId.Value;
                    }
                }
            }
            else
            {
                entityEntry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                entityEntry.Property(nameof(BaseEntity.CreatedById)).IsModified = false;
            }

            entity.UpdatedAt = DateTime.UtcNow;
            if (_userLookupService != null)
            {
                var userId = _userLookupService.GetCurrentUserId();
                if (userId.HasValue)
                {
                    entity.UpdatedById = (Guid)userId.Value;
                }
            }
        }
    }
} 