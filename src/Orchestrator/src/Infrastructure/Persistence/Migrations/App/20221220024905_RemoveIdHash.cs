using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class RemoveIdHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Things_IdHash",
                schema: "truquest",
                table: "Things");

            migrationBuilder.DropIndex(
                name: "IX_SettlementProposals_IdHash",
                schema: "truquest",
                table: "SettlementProposals");

            migrationBuilder.DropColumn(
                name: "IdHash",
                schema: "truquest",
                table: "Things");

            migrationBuilder.DropColumn(
                name: "IdHash",
                schema: "truquest",
                table: "SettlementProposals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdHash",
                schema: "truquest",
                table: "Things",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdHash",
                schema: "truquest",
                table: "SettlementProposals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Things_IdHash",
                schema: "truquest",
                table: "Things",
                column: "IdHash");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposals_IdHash",
                schema: "truquest",
                table: "SettlementProposals",
                column: "IdHash");
        }
    }
}
