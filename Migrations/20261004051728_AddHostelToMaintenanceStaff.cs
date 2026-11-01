using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hostel_hub.Migrations
{
    /// <inheritdoc />
    public partial class AddHostelToMaintenanceStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HostelId",
                table: "MaintenanceStaff",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceStaff_HostelId",
                table: "MaintenanceStaff",
                column: "HostelId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceStaff_Hostels_HostelId",
                table: "MaintenanceStaff",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "HostelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceStaff_Hostels_HostelId",
                table: "MaintenanceStaff");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceStaff_HostelId",
                table: "MaintenanceStaff");

            migrationBuilder.DropColumn(
                name: "HostelId",
                table: "MaintenanceStaff");
        }
    }
}
