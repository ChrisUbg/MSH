using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MSH.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbSchemaBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "db");

            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UpdatedById = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LastName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirmwareUpdate",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CurrentVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DownloadUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Checksum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DownloadStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DownloadCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InstallationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InstallationCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdateMetadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    IsCompatible = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresConfirmation = table.Column<bool>(type: "boolean", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmwareUpdate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Floor = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Condition = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Action = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentalSettings",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IndoorTemperatureMin = table.Column<double>(type: "double precision", nullable: false),
                    IndoorTemperatureMax = table.Column<double>(type: "double precision", nullable: false),
                    OutdoorTemperatureMin = table.Column<double>(type: "double precision", nullable: false),
                    OutdoorTemperatureMax = table.Column<double>(type: "double precision", nullable: false),
                    HumidityMin = table.Column<double>(type: "double precision", nullable: false),
                    HumidityMax = table.Column<double>(type: "double precision", nullable: false),
                    CO2Max = table.Column<double>(type: "double precision", nullable: false),
                    VOCMax = table.Column<double>(type: "double precision", nullable: false),
                    TemperatureWarning = table.Column<double>(type: "double precision", nullable: false),
                    HumidityWarning = table.Column<double>(type: "double precision", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentalSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvironmentalSettings_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "db",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                schema: "db",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Theme = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Language = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ShowOfflineDevices = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultView = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DashboardLayout = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    FavoriteDevices = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    QuickActions = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    EmailNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    PushNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationPreferences = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    DeviceDisplayPreferences = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    LastUsedDevices = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    RoomDisplayOrder = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    ShowEmptyRooms = table.Column<bool>(type: "boolean", nullable: false),
                    ShowAutomationSuggestions = table.Column<bool>(type: "boolean", nullable: false),
                    AutomationPreferences = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettings_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "db",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "db",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "db",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "db",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "db",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "db",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "db",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "db",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "db",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "db",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupStateHistory",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldState = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    NewState = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupStateHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupStateHistory_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "db",
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupStates",
                schema: "db",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupStates", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_GroupStates_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "db",
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceGroups",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Icon = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PreferredCommissioningMethod = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DefaultCapabilities = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceGroups_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "db",
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoomPermissions",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoomPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoomPermissions_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "db",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoomPermissions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "db",
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuleActions",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleActions_Rules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "db",
                        principalTable: "Rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuleConditions",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleConditions_Rules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "db",
                        principalTable: "Rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuleExecutionHistory",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Result = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleExecutionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleExecutionHistory_Rules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "db",
                        principalTable: "Rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuleTriggers",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Trigger = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastTriggered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleTriggers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleTriggers_Rules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "db",
                        principalTable: "Rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTypes",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Capabilities = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    IsSimulated = table.Column<bool>(type: "boolean", nullable: false),
                    Icon = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PreferredCommissioningMethod = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DeviceGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTypes_DeviceGroups_DeviceGroupId",
                        column: x => x.DeviceGroupId,
                        principalSchema: "db",
                        principalTable: "DeviceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MatterDeviceId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DeviceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    Properties = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Configuration = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    LastStateChange = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceTypes_DeviceTypeId",
                        column: x => x.DeviceTypeId,
                        principalSchema: "db",
                        principalTable: "DeviceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devices_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "db",
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceDeviceGroup",
                schema: "db",
                columns: table => new
                {
                    DeviceGroupsId = table.Column<Guid>(type: "uuid", nullable: false),
                    DevicesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDeviceGroup", x => new { x.DeviceGroupsId, x.DevicesId });
                    table.ForeignKey(
                        name: "FK_DeviceDeviceGroup_DeviceGroups_DeviceGroupsId",
                        column: x => x.DeviceGroupsId,
                        principalSchema: "db",
                        principalTable: "DeviceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceDeviceGroup_Devices_DevicesId",
                        column: x => x.DevicesId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEventLogs",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Event = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EventData = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    OldState = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    NewState = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEventLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceEventLogs_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEvents",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EventData = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceFirmwareUpdate",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirmwareUpdateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DownloadStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DownloadCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InstallationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InstallationCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TestCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TestPassed = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TestResults = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsRollbackAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    RollbackCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RollbackReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdateLog = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFirmwareUpdate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceFirmwareUpdate_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceFirmwareUpdate_FirmwareUpdate_FirmwareUpdateId",
                        column: x => x.FirmwareUpdateId,
                        principalSchema: "db",
                        principalTable: "FirmwareUpdate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceHistory",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    OldState = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    NewState = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceHistory_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevicePropertyChange",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    NewValue = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    ChangeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ChangeTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePropertyChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevicePropertyChange_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceStates",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StateValue = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceStates_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                schema: "db",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => new { x.GroupId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_GroupMembers_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "db",
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDevicePermissions",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionLevel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevicePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevicePermissions_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "db",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDevicePermissions_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "db",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_Email",
                schema: "db",
                table: "ApplicationUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_UserName",
                schema: "db",
                table: "ApplicationUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "db",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "db",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "db",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "db",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "db",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "db",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "db",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDeviceGroup_DevicesId",
                schema: "db",
                table: "DeviceDeviceGroup",
                column: "DevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEventLogs_DeviceId",
                schema: "db",
                table: "DeviceEventLogs",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEventLogs_EventType",
                schema: "db",
                table: "DeviceEventLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEventLogs_Timestamp",
                schema: "db",
                table: "DeviceEventLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_DeviceId",
                schema: "db",
                table: "DeviceEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFirmwareUpdate_DeviceId",
                schema: "db",
                table: "DeviceFirmwareUpdate",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFirmwareUpdate_FirmwareUpdateId",
                schema: "db",
                table: "DeviceFirmwareUpdate",
                column: "FirmwareUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGroups_RoomId",
                schema: "db",
                table: "DeviceGroups",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceHistory_DeviceId",
                schema: "db",
                table: "DeviceHistory",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePropertyChange_DeviceId",
                schema: "db",
                table: "DevicePropertyChange",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceTypeId",
                schema: "db",
                table: "Devices",
                column: "DeviceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_MatterDeviceId",
                schema: "db",
                table: "Devices",
                column: "MatterDeviceId",
                unique: true,
                filter: "\"MatterDeviceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_RoomId",
                schema: "db",
                table: "Devices",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStates_DeviceId",
                schema: "db",
                table: "DeviceStates",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTypes_DeviceGroupId",
                schema: "db",
                table: "DeviceTypes",
                column: "DeviceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentalSettings_UserId",
                schema: "db",
                table: "EnvironmentalSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_DeviceId",
                schema: "db",
                table: "GroupMembers",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupStateHistory_GroupId",
                schema: "db",
                table: "GroupStateHistory",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleActions_RuleId",
                schema: "db",
                table: "RuleActions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleConditions_RuleId",
                schema: "db",
                table: "RuleConditions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleExecutionHistory_RuleId",
                schema: "db",
                table: "RuleExecutionHistory",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTriggers_RuleId",
                schema: "db",
                table: "RuleTriggers",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevicePermissions_DeviceId",
                schema: "db",
                table: "UserDevicePermissions",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevicePermissions_UserId",
                schema: "db",
                table: "UserDevicePermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoomPermissions_RoomId",
                schema: "db",
                table: "UserRoomPermissions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoomPermissions_UserId",
                schema: "db",
                table: "UserRoomPermissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "db");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "db");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "db");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "db");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DeviceDeviceGroup",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DeviceEventLogs",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DeviceEvents",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DeviceFirmwareUpdate",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DeviceHistory",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DevicePropertyChange",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DeviceStates",
                schema: "db");

            migrationBuilder.DropTable(
                name: "EnvironmentalSettings",
                schema: "db");

            migrationBuilder.DropTable(
                name: "GroupMembers",
                schema: "db");

            migrationBuilder.DropTable(
                name: "GroupStateHistory",
                schema: "db");

            migrationBuilder.DropTable(
                name: "GroupStates",
                schema: "db");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "db");

            migrationBuilder.DropTable(
                name: "RuleActions",
                schema: "db");

            migrationBuilder.DropTable(
                name: "RuleConditions",
                schema: "db");

            migrationBuilder.DropTable(
                name: "RuleExecutionHistory",
                schema: "db");

            migrationBuilder.DropTable(
                name: "RuleTriggers",
                schema: "db");

            migrationBuilder.DropTable(
                name: "UserDevicePermissions",
                schema: "db");

            migrationBuilder.DropTable(
                name: "UserRoomPermissions",
                schema: "db");

            migrationBuilder.DropTable(
                name: "UserSettings",
                schema: "db");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "db");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "db");

            migrationBuilder.DropTable(
                name: "FirmwareUpdate",
                schema: "db");

            migrationBuilder.DropTable(
                name: "Groups",
                schema: "db");

            migrationBuilder.DropTable(
                name: "Rules",
                schema: "db");

            migrationBuilder.DropTable(
                name: "Devices",
                schema: "db");

            migrationBuilder.DropTable(
                name: "ApplicationUsers",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DeviceTypes",
                schema: "db");

            migrationBuilder.DropTable(
                name: "DeviceGroups",
                schema: "db");

            migrationBuilder.DropTable(
                name: "Rooms",
                schema: "db");
        }
    }
}
