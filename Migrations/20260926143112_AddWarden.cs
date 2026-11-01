using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hostel_hub.Migrations
{
    /// <inheritdoc />
    public partial class AddWarden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wardens",
                columns: table => new
                {
                    WardenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HostelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wardens", x => x.WardenId);
                    table.ForeignKey(
                        name: "FK_Wardens_Hostels_HostelId",
                        column: x => x.HostelId,
                        principalTable: "Hostels",
                        principalColumn: "HostelId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Wardens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wardens_HostelId",
                table: "Wardens",
                column: "HostelId",
                unique: true,
                filter: "[HostelId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wardens_UserId",
                table: "Wardens",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wardens");
        }
    }
}
