using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hostel_hub.Migrations
{
    /// <inheritdoc />
    public partial class RestrictHostelRoomDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Hostels_HostelId",
                table: "Rooms");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Hostels_HostelId",
                table: "Rooms",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "HostelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Hostels_HostelId",
                table: "Rooms");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Hostels_HostelId",
                table: "Rooms",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "HostelId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
