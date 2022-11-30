using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddTaskAndLastUpdatedAtToThing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastUpdatedAtBlockNumber",
                schema: "truquest",
                table: "Things",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ScheduledBlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    Payload = table.Column<IReadOnlyDictionary<string, object>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "truquest");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAtBlockNumber",
                schema: "truquest",
                table: "Things");
        }
    }
}
