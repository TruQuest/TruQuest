using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdHashToThing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdHash",
                schema: "truquest",
                table: "Things",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Things_IdHash",
                schema: "truquest",
                table: "Things",
                column: "IdHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Things_IdHash",
                schema: "truquest",
                table: "Things");

            migrationBuilder.DropColumn(
                name: "IdHash",
                schema: "truquest",
                table: "Things");
        }
    }
}
