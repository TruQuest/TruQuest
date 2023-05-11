using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddWatchList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettlementProposalUpdates",
                schema: "truquest",
                columns: table => new
                {
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementProposalUpdates", x => new { x.SettlementProposalId, x.UpdateTimestamp });
                });

            migrationBuilder.CreateTable(
                name: "SubjectUpdates",
                schema: "truquest",
                columns: table => new
                {
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectUpdates", x => new { x.SubjectId, x.UpdateTimestamp });
                });

            migrationBuilder.CreateTable(
                name: "ThingUpdates",
                schema: "truquest",
                columns: table => new
                {
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingUpdates", x => new { x.ThingId, x.UpdateTimestamp });
                });

            migrationBuilder.CreateTable(
                name: "WatchList",
                schema: "truquest",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCheckedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchList", x => new { x.UserId, x.ItemType, x.ItemId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettlementProposalUpdates",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "SubjectUpdates",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "ThingUpdates",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "WatchList",
                schema: "truquest");
        }
    }
}
