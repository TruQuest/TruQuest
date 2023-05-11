using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddTitleToItemUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Details",
                schema: "truquest",
                table: "ThingUpdates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "truquest",
                table: "ThingUpdates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                schema: "truquest",
                table: "SubjectUpdates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "truquest",
                table: "SubjectUpdates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                schema: "truquest",
                table: "SettlementProposalUpdates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "truquest",
                table: "SettlementProposalUpdates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                schema: "truquest",
                table: "ThingUpdates");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "truquest",
                table: "SubjectUpdates");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "truquest",
                table: "SettlementProposalUpdates");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                schema: "truquest",
                table: "ThingUpdates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                schema: "truquest",
                table: "SubjectUpdates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                schema: "truquest",
                table: "SettlementProposalUpdates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
