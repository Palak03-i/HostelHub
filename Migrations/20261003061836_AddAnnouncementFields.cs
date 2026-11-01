using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hostel_hub.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Users_PostedByUserId",
                table: "Announcements");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                table: "Announcements",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Announcements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImportant",
                table: "Announcements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PublishDate",
                table: "Announcements",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Users_PostedByUserId",
                table: "Announcements",
                column: "PostedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Users_PostedByUserId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "IsImportant",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "PublishDate",
                table: "Announcements");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Users_PostedByUserId",
                table: "Announcements",
                column: "PostedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
