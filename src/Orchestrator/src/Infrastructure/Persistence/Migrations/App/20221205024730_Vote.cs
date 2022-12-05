using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class Vote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Votes",
                schema: "truquest",
                columns: table => new
                {
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<string>(type: "text", nullable: false),
                    PollType = table.Column<int>(type: "integer", nullable: false),
                    CastedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    VoterSignature = table.Column<string>(type: "text", nullable: false),
                    IpfsCid = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => new { x.ThingId, x.VoterId });
                    table.ForeignKey(
                        name: "FK_Votes_ThingVerifiers_ThingId_VoterId",
                        columns: x => new { x.ThingId, x.VoterId },
                        principalSchema: "truquest",
                        principalTable: "ThingVerifiers",
                        principalColumns: new[] { "ThingId", "VerifierId" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votes",
                schema: "truquest");
        }
    }
}
