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
    
    // New entities for firmware updates and clusters
    public DbSet<Cluster> Clusters { get; set; } = null!;
    public DbSet<DevicePropertyChange> DevicePropertyChanges { get; set; } = null!;
    public DbSet<FirmwareUpdate> FirmwareUpdates { get; set; } = null!;
    public DbSet<DeviceFirmwareUpdate> DeviceFirmwareUpdates { get; set; } = null!;
    
    // Event log entity
    public DbSet<DeviceEventLog> DeviceEventLogs { get; set; } = null!;
    
    // Device action delay entity
    public DbSet<DeviceActionDelay> DeviceActionDelays { get; set; } = null!;
    public DbSet<DeviceEventDelay> DeviceEventDelays { get; set; } = null!;

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
            .HasOne(us => us.User)
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

        // Configure DeviceGroup and DeviceType relationship
        modelBuilder.Entity<DeviceType>()
            .HasOne(dt => dt.DeviceGroup)
            .WithMany(dg => dg.DeviceTypes)
            .HasForeignKey(dt => dt.DeviceGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeviceGroup>()
            .Property(dg => dg.DefaultCapabilities)
            .HasColumnType("jsonb");

        modelBuilder.Entity<DeviceType>()
            .Property(dt => dt.Capabilities)
            .HasColumnType("jsonb");

        // modelBuilder.Entity<DeviceGroupMember>()
        //     .HasKey(dgm => new { dgm.DeviceId, dgm.DeviceGroupId });

        // modelBuilder.Entity<DeviceGroupMember>()
        //     .HasOne(dgm => dgm.Device)
        //     .WithMany(d => d.DeviceGroupMembers)
        //     .HasForeignKey(dgm => dgm.DeviceId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<DeviceGroupMember>()
        //     .HasOne(dgm => dgm.DeviceGroup)
        //     .WithMany(dg => dg.DeviceGroupMembers)
        //     .HasForeignKey(dgm => dgm.DeviceGroupId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // Configure DeviceGroupMember navigation properties
        // modelBuilder.Entity<DeviceGroupMember>()
        //     .HasOne(dgm => dgm.CreatedBy)
        //     .WithMany()
        //     .HasForeignKey(dgm => dgm.CreatedById)
        //     .OnDelete(DeleteBehavior.Restrict);

        // modelBuilder.Entity<DeviceGroupMember>()
        //     .HasOne(dgm => dgm.UpdatedBy)
        //     .WithMany()
        //     .HasForeignKey(dgm => dgm.UpdatedById)
        //     .OnDelete(DeleteBehavior.Restrict);

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

        // Configure many-to-many relationship between Device and DeviceGroup
        modelBuilder.Entity<Device>()
            .HasMany(d => d.DeviceGroups)
            .WithMany(dg => dg.Devices)
            .UsingEntity(j => j.ToTable("DeviceDeviceGroup"));

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
            .HasOne(gm => gm.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(gm => gm.GroupId);

        modelBuilder.Entity<GroupMember>()
            .HasOne(gm => gm.Device)
            .WithMany(d => d.GroupMembers)
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
            .HasOne(gsh => gsh.Group)
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
            .HasOne(rc => rc.Rule)
            .WithMany()
            .HasForeignKey(rc => rc.RuleId);

        modelBuilder.Entity<RuleCondition>()
            .Property(rc => rc.Condition)
            .HasColumnType("jsonb");

        // Configure RuleAction
        modelBuilder.Entity<RuleAction>()
            .HasKey(ra => ra.Id);

        modelBuilder.Entity<RuleAction>()
            .HasOne(ra => ra.Rule)
            .WithMany()
            .HasForeignKey(ra => ra.RuleId);

        modelBuilder.Entity<RuleAction>()
            .Property(ra => ra.Action)
            .HasColumnType("jsonb");

        // Configure RuleTrigger
        modelBuilder.Entity<RuleTrigger>()
            .HasKey(rt => rt.Id);

        // RuleTrigger relationship is configured via [ForeignKey] attribute

        modelBuilder.Entity<RuleTrigger>()
            .Property(rt => rt.Trigger)
            .HasColumnType("jsonb");

        // Configure RuleExecutionHistory
        modelBuilder.Entity<RuleExecutionHistory>()
            .HasKey(reh => reh.Id);

        modelBuilder.Entity<RuleExecutionHistory>()
            .HasOne(reh => reh.Rule)
            .WithMany()
            .HasForeignKey(reh => reh.RuleId);

        modelBuilder.Entity<RuleExecutionHistory>()
            .Property(reh => reh.Result)
            .HasColumnType("jsonb");
            
        // Configure Cluster
        modelBuilder.Entity<Cluster>()
            .HasKey(c => c.Id);
            
        modelBuilder.Entity<Cluster>()
            .HasIndex(c => c.ClusterId)
            .IsUnique();
            
        modelBuilder.Entity<Cluster>()
            .Property(c => c.Attributes)
            .HasColumnType("jsonb");
            
        modelBuilder.Entity<Cluster>()
            .Property(c => c.Commands)
            .HasColumnType("jsonb");
            
        modelBuilder.Entity<Cluster>()
            .Property(c => c.Events)
            .HasColumnType("jsonb");
            
        // Configure DevicePropertyChange
        modelBuilder.Entity<DevicePropertyChange>()
            .HasKey(dpc => dpc.Id);
            
        modelBuilder.Entity<DevicePropertyChange>()
            .HasOne(dpc => dpc.Device)
            .WithMany(d => d.PropertyChanges)
            .HasForeignKey(dpc => dpc.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<DevicePropertyChange>()
            .Property(dpc => dpc.OldValue)
            .HasColumnType("jsonb");
            
        modelBuilder.Entity<DevicePropertyChange>()
            .Property(dpc => dpc.NewValue)
            .HasColumnType("jsonb");
            
        // Configure FirmwareUpdate
        modelBuilder.Entity<FirmwareUpdate>()
            .HasKey(fu => fu.Id);
            
        modelBuilder.Entity<FirmwareUpdate>()
            .Property(fu => fu.UpdateMetadata)
            .HasColumnType("jsonb");
            
        // Configure DeviceFirmwareUpdate
        modelBuilder.Entity<DeviceFirmwareUpdate>()
            .HasKey(dfu => dfu.Id);
            
        modelBuilder.Entity<DeviceFirmwareUpdate>()
            .HasOne(dfu => dfu.Device)
            .WithMany(d => d.FirmwareUpdates)
            .HasForeignKey(dfu => dfu.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<DeviceFirmwareUpdate>()
            .HasOne(dfu => dfu.FirmwareUpdate)
            .WithMany(fu => fu.DeviceUpdates)
            .HasForeignKey(dfu => dfu.FirmwareUpdateId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<DeviceFirmwareUpdate>()
            .Property(dfu => dfu.TestResults)
            .HasColumnType("jsonb");
            
        modelBuilder.Entity<DeviceFirmwareUpdate>()
            .Property(dfu => dfu.UpdateLog)
            .HasColumnType("jsonb");
            
        // Configure DeviceEventLog
        modelBuilder.Entity<DeviceEventLog>()
            .HasKey(del => del.Id);
            
        modelBuilder.Entity<DeviceEventLog>()
            .HasOne(del => del.Device)
            .WithMany(d => d.EventLogs)
            .HasForeignKey(del => del.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<DeviceEventLog>()
            .Property(del => del.EventData)
            .HasColumnType("jsonb");
            
        modelBuilder.Entity<DeviceEventLog>()
            .Property(del => del.OldState)
            .HasColumnType("jsonb");
            
        modelBuilder.Entity<DeviceEventLog>()
            .Property(del => del.NewState)
            .HasColumnType("jsonb");
            
        modelBuilder.Entity<DeviceEventLog>()
            .HasIndex(del => del.DeviceId);
            
        modelBuilder.Entity<DeviceEventLog>()
            .HasIndex(del => del.Timestamp);
            
        modelBuilder.Entity<DeviceEventLog>()
            .HasIndex(del => del.EventType);
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
                    if (userId != null)
                    {
                        entity.CreatedById = userId;
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
                if (userId != null)
                {
                    entity.UpdatedById = userId;
                }
            }
        }
    }
} 