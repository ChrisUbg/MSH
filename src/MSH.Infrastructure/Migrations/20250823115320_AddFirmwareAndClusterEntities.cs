using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSH.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmwareAndClusterEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceFirmwareUpdate_Devices_DeviceId",
                schema: "db",
                table: "DeviceFirmwareUpdate");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceFirmwareUpdate_FirmwareUpdate_FirmwareUpdateId",
                schema: "db",
                table: "DeviceFirmwareUpdate");

            migrationBuilder.DropForeignKey(
                name: "FK_DevicePropertyChange_Devices_DeviceId",
                schema: "db",
                table: "DevicePropertyChange");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FirmwareUpdate",
                schema: "db",
                table: "FirmwareUpdate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevicePropertyChange",
                schema: "db",
                table: "DevicePropertyChange");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceFirmwareUpdate",
                schema: "db",
                table: "DeviceFirmwareUpdate");

            migrationBuilder.RenameTable(
                name: "FirmwareUpdate",
                schema: "db",
                newName: "FirmwareUpdates",
                newSchema: "db");

            migrationBuilder.RenameTable(
                name: "DevicePropertyChange",
                schema: "db",
                newName: "DevicePropertyChanges",
                newSchema: "db");

            migrationBuilder.RenameTable(
                name: "DeviceFirmwareUpdate",
                schema: "db",
                newName: "DeviceFirmwareUpdates",
                newSchema: "db");

            migrationBuilder.RenameIndex(
                name: "IX_DevicePropertyChange_DeviceId",
                schema: "db",
                table: "DevicePropertyChanges",
                newName: "IX_DevicePropertyChanges_DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceFirmwareUpdate_FirmwareUpdateId",
                schema: "db",
                table: "DeviceFirmwareUpdates",
                newName: "IX_DeviceFirmwareUpdates_FirmwareUpdateId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceFirmwareUpdate_DeviceId",
                schema: "db",
                table: "DeviceFirmwareUpdates",
                newName: "IX_DeviceFirmwareUpdates_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FirmwareUpdates",
                schema: "db",
                table: "FirmwareUpdates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevicePropertyChanges",
                schema: "db",
                table: "DevicePropertyChanges",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceFirmwareUpdates",
                schema: "db",
                table: "DeviceFirmwareUpdates",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Clusters",
                schema: "db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClusterId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false),
                    MatterVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Attributes = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Commands = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Events = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    UpdatedById = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_ClusterId",
                schema: "db",
                table: "Clusters",
                column: "ClusterId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceFirmwareUpdates_Devices_DeviceId",
                schema: "db",
                table: "DeviceFirmwareUpdates",
                column: "DeviceId",
                principalSchema: "db",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceFirmwareUpdates_FirmwareUpdates_FirmwareUpdateId",
                schema: "db",
                table: "DeviceFirmwareUpdates",
                column: "FirmwareUpdateId",
                principalSchema: "db",
                principalTable: "FirmwareUpdates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DevicePropertyChanges_Devices_DeviceId",
                schema: "db",
                table: "DevicePropertyChanges",
                column: "DeviceId",
                principalSchema: "db",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceFirmwareUpdates_Devices_DeviceId",
                schema: "db",
                table: "DeviceFirmwareUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceFirmwareUpdates_FirmwareUpdates_FirmwareUpdateId",
                schema: "db",
                table: "DeviceFirmwareUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_DevicePropertyChanges_Devices_DeviceId",
                schema: "db",
                table: "DevicePropertyChanges");

            migrationBuilder.DropTable(
                name: "Clusters",
                schema: "db");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FirmwareUpdates",
                schema: "db",
                table: "FirmwareUpdates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevicePropertyChanges",
                schema: "db",
                table: "DevicePropertyChanges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceFirmwareUpdates",
                schema: "db",
                table: "DeviceFirmwareUpdates");

            migrationBuilder.RenameTable(
                name: "FirmwareUpdates",
                schema: "db",
                newName: "FirmwareUpdate",
                newSchema: "db");

            migrationBuilder.RenameTable(
                name: "DevicePropertyChanges",
                schema: "db",
                newName: "DevicePropertyChange",
                newSchema: "db");

            migrationBuilder.RenameTable(
                name: "DeviceFirmwareUpdates",
                schema: "db",
                newName: "DeviceFirmwareUpdate",
                newSchema: "db");

            migrationBuilder.RenameIndex(
                name: "IX_DevicePropertyChanges_DeviceId",
                schema: "db",
                table: "DevicePropertyChange",
                newName: "IX_DevicePropertyChange_DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceFirmwareUpdates_FirmwareUpdateId",
                schema: "db",
                table: "DeviceFirmwareUpdate",
                newName: "IX_DeviceFirmwareUpdate_FirmwareUpdateId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceFirmwareUpdates_DeviceId",
                schema: "db",
                table: "DeviceFirmwareUpdate",
                newName: "IX_DeviceFirmwareUpdate_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FirmwareUpdate",
                schema: "db",
                table: "FirmwareUpdate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevicePropertyChange",
                schema: "db",
                table: "DevicePropertyChange",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceFirmwareUpdate",
                schema: "db",
                table: "DeviceFirmwareUpdate",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceFirmwareUpdate_Devices_DeviceId",
                schema: "db",
                table: "DeviceFirmwareUpdate",
                column: "DeviceId",
                principalSchema: "db",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceFirmwareUpdate_FirmwareUpdate_FirmwareUpdateId",
                schema: "db",
                table: "DeviceFirmwareUpdate",
                column: "FirmwareUpdateId",
                principalSchema: "db",
                principalTable: "FirmwareUpdate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DevicePropertyChange_Devices_DeviceId",
                schema: "db",
                table: "DevicePropertyChange",
                column: "DeviceId",
                principalSchema: "db",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
