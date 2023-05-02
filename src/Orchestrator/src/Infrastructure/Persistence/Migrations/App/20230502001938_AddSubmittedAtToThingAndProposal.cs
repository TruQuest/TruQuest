using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddSubmittedAtToThingAndProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SubmittedAt",
                schema: "truquest",
                table: "Things",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SubmittedAt",
                schema: "truquest",
                table: "SettlementProposals",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                schema: "truquest",
                table: "Things");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                schema: "truquest",
                table: "SettlementProposals");
        }
    }
}
