using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class ChangeWhitelistEntryPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Whitelist",
                schema: "truquest",
                table: "Whitelist");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "truquest",
                table: "Whitelist");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Whitelist",
                schema: "truquest",
                table: "Whitelist",
                columns: new[] { "Type", "Value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Whitelist",
                schema: "truquest",
                table: "Whitelist");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "truquest",
                table: "Whitelist",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Whitelist",
                schema: "truquest",
                table: "Whitelist",
                column: "Id");
        }
    }
}
