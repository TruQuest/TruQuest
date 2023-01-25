using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddImageToSettlementProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CroppedImageIpfsCid",
                schema: "truquest",
                table: "SettlementProposals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageIpfsCid",
                schema: "truquest",
                table: "SettlementProposals",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CroppedImageIpfsCid",
                schema: "truquest",
                table: "SettlementProposals");

            migrationBuilder.DropColumn(
                name: "ImageIpfsCid",
                schema: "truquest",
                table: "SettlementProposals");
        }
    }
}
