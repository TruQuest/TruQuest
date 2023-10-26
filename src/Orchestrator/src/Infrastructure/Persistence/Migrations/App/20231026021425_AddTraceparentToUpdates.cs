using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddTraceparentToUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Traceparent",
                schema: "truquest",
                table: "ThingUpdates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Traceparent",
                schema: "truquest",
                table: "SubjectUpdates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Traceparent",
                schema: "truquest",
                table: "SettlementProposalUpdates",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Traceparent",
                schema: "truquest",
                table: "ThingUpdates");

            migrationBuilder.DropColumn(
                name: "Traceparent",
                schema: "truquest",
                table: "SubjectUpdates");

            migrationBuilder.DropColumn(
                name: "Traceparent",
                schema: "truquest",
                table: "SettlementProposalUpdates");
        }
    }
}
