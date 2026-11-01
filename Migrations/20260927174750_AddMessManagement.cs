using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hostel_hub.Migrations
{
    /// <inheritdoc />
    public partial class AddMessManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessMenus",
                columns: table => new
                {
                    MessMenuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostelId = table.Column<int>(type: "int", nullable: false),
                    MenuDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsBreakfastAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsLunchAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsDinnerAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessMenus", x => x.MessMenuId);
                    table.ForeignKey(
                        name: "FK_MessMenus_Hostels_HostelId",
                        column: x => x.HostelId,
                        principalTable: "Hostels",
                        principalColumn: "HostelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessMenus_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealSelections",
                columns: table => new
                {
                    MealSelectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    MessMenuId = table.Column<int>(type: "int", nullable: false),
                    MealType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SelectedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealSelections", x => x.MealSelectionId);
                    table.ForeignKey(
                        name: "FK_MealSelections_MessMenus_MessMenuId",
                        column: x => x.MessMenuId,
                        principalTable: "MessMenus",
                        principalColumn: "MessMenuId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealSelections_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessMenuItems",
                columns: table => new
                {
                    MessMenuItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessMenuId = table.Column<int>(type: "int", nullable: false),
                    MealType = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessMenuItems", x => x.MessMenuItemId);
                    table.ForeignKey(
                        name: "FK_MessMenuItems_MessMenus_MessMenuId",
                        column: x => x.MessMenuId,
                        principalTable: "MessMenus",
                        principalColumn: "MessMenuId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealSelections_MessMenuId",
                table: "MealSelections",
                column: "MessMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MealSelections_StudentId_MessMenuId_MealType",
                table: "MealSelections",
                columns: new[] { "StudentId", "MessMenuId", "MealType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessMenuItems_MessMenuId",
                table: "MessMenuItems",
                column: "MessMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MessMenus_CreatedByUserId",
                table: "MessMenus",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessMenus_HostelId_MenuDate",
                table: "MessMenus",
                columns: new[] { "HostelId", "MenuDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealSelections");

            migrationBuilder.DropTable(
                name: "MessMenuItems");

            migrationBuilder.DropTable(
                name: "MessMenus");
        }
    }
}
