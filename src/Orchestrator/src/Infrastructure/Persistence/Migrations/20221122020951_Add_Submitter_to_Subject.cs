using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmittertoSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmitterId",
                schema: "truquest",
                table: "Subjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SubmitterId",
                schema: "truquest",
                table: "Subjects",
                column: "SubmitterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AspNetUsers_SubmitterId",
                schema: "truquest",
                table: "Subjects",
                column: "SubmitterId",
                principalSchema: "truquest",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AspNetUsers_SubmitterId",
                schema: "truquest",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_SubmitterId",
                schema: "truquest",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SubmitterId",
                schema: "truquest",
                table: "Subjects");
        }
    }
}
