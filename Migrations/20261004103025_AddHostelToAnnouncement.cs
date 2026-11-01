using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hostel_hub.Migrations
{
    /// <inheritdoc />
    public partial class AddHostelToAnnouncement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HostelId",
                table: "Announcements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_HostelId",
                table: "Announcements",
                column: "HostelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Hostels_HostelId",
                table: "Announcements",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "HostelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Hostels_HostelId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_HostelId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "HostelId",
                table: "Announcements");
        }
    }
}
