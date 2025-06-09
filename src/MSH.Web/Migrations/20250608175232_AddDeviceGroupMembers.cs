using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MSH.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceGroupMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGroupMembers_DeviceGroups_GroupId",
                table: "DeviceGroupMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceGroupMembers",
                table: "DeviceGroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGroupMembers_DeviceId",
                table: "DeviceGroupMembers");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "DeviceGroupMembers",
                newName: "DeviceGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceGroupMembers_GroupId",
                table: "DeviceGroupMembers",
                newName: "IX_DeviceGroupMembers_DeviceGroupId");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Devices",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DeviceGroupMembers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceGroupMembers",
                table: "DeviceGroupMembers",
                columns: new[] { "DeviceId", "DeviceGroupId" });

            migrationBuilder.CreateTable(
                name: "DeviceDeviceGroup",
                columns: table => new
                {
                    DeviceGroupsId = table.Column<int>(type: "integer", nullable: false),
                    DevicesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDeviceGroup", x => new { x.DeviceGroupsId, x.DevicesId });
                    table.ForeignKey(
                        name: "FK_DeviceDeviceGroup_DeviceGroups_DeviceGroupsId",
                        column: x => x.DeviceGroupsId,
                        principalTable: "DeviceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceDeviceGroup_Devices_DevicesId",
                        column: x => x.DevicesId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDeviceGroup_DevicesId",
                table: "DeviceDeviceGroup",
                column: "DevicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGroupMembers_DeviceGroups_DeviceGroupId",
                table: "DeviceGroupMembers",
                column: "DeviceGroupId",
                principalTable: "DeviceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGroupMembers_DeviceGroups_DeviceGroupId",
                table: "DeviceGroupMembers");

            migrationBuilder.DropTable(
                name: "DeviceDeviceGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceGroupMembers",
                table: "DeviceGroupMembers");

            migrationBuilder.RenameColumn(
                name: "DeviceGroupId",
                table: "DeviceGroupMembers",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceGroupMembers_DeviceGroupId",
                table: "DeviceGroupMembers",
                newName: "IX_DeviceGroupMembers_GroupId");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DeviceGroupMembers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceGroupMembers",
                table: "DeviceGroupMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGroupMembers_DeviceId",
                table: "DeviceGroupMembers",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGroupMembers_DeviceGroups_GroupId",
                table: "DeviceGroupMembers",
                column: "GroupId",
                principalTable: "DeviceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
