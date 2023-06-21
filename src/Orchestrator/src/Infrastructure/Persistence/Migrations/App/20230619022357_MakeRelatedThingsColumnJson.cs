using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class MakeRelatedThingsColumnJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedThingId",
                schema: "truquest",
                table: "Things");

            migrationBuilder.AddColumn<IReadOnlyDictionary<string, string>>(
                name: "RelatedThings",
                schema: "truquest",
                table: "Things",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedThings",
                schema: "truquest",
                table: "Things");

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedThingId",
                schema: "truquest",
                table: "Things",
                type: "uuid",
                nullable: true);
        }
    }
}
