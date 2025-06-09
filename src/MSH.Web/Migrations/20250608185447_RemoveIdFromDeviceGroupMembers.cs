using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSH.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdFromDeviceGroupMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceGroupMembers",
                table: "DeviceGroupMembers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DeviceGroupMembers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceGroupMembers",
                table: "DeviceGroupMembers",
                columns: new[] { "DeviceId", "DeviceGroupId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceGroupMembers",
                table: "DeviceGroupMembers");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DeviceGroupMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceGroupMembers",
                table: "DeviceGroupMembers",
                column: "Id");
        }
    }
}
