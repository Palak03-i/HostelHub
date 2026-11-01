using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hostel_hub.Migrations
{
    /// <inheritdoc />
    public partial class ReDesignFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Complaints_ComplaintId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_ComplaintId",
                table: "Feedbacks");

            migrationBuilder.AlterColumn<int>(
                name: "ComplaintId",
                table: "Feedbacks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Feedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "Feedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ComplaintId",
                table: "Feedbacks",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_StudentId_ComplaintId",
                table: "Feedbacks",
                columns: new[] { "StudentId", "ComplaintId" },
                unique: true,
                filter: "[ComplaintId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Complaints_ComplaintId",
                table: "Feedbacks",
                column: "ComplaintId",
                principalTable: "Complaints",
                principalColumn: "ComplaintId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Students_StudentId",
                table: "Feedbacks",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Complaints_ComplaintId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Students_StudentId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_ComplaintId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_StudentId_ComplaintId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Feedbacks");

            migrationBuilder.AlterColumn<int>(
                name: "ComplaintId",
                table: "Feedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ComplaintId",
                table: "Feedbacks",
                column: "ComplaintId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Complaints_ComplaintId",
                table: "Feedbacks",
                column: "ComplaintId",
                principalTable: "Complaints",
                principalColumn: "ComplaintId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
