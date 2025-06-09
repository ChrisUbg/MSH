using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSH.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentToDeviceGroupMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGroupMembers_Users_CreatedById",
                table: "DeviceGroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGroupMembers_Users_UpdatedById",
                table: "DeviceGroupMembers");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "DeviceGroupMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGroupMembers_Users_CreatedById",
                table: "DeviceGroupMembers",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGroupMembers_Users_UpdatedById",
                table: "DeviceGroupMembers",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGroupMembers_Users_CreatedById",
                table: "DeviceGroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGroupMembers_Users_UpdatedById",
                table: "DeviceGroupMembers");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "DeviceGroupMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGroupMembers_Users_CreatedById",
                table: "DeviceGroupMembers",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGroupMembers_Users_UpdatedById",
                table: "DeviceGroupMembers",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
