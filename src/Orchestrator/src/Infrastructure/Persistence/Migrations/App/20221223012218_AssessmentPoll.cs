using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AssessmentPoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votes",
                schema: "truquest");

            migrationBuilder.CreateTable(
                name: "AcceptancePollVotes",
                schema: "truquest",
                columns: table => new
                {
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<string>(type: "text", nullable: false),
                    CastedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    VoterSignature = table.Column<string>(type: "text", nullable: false),
                    IpfsCid = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcceptancePollVotes", x => new { x.ThingId, x.VoterId });
                    table.ForeignKey(
                        name: "FK_AcceptancePollVotes_ThingVerifiers_ThingId_VoterId",
                        columns: x => new { x.ThingId, x.VoterId },
                        principalSchema: "truquest",
                        principalTable: "ThingVerifiers",
                        principalColumns: new[] { "ThingId", "VerifierId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementProposalVerifiers",
                schema: "truquest",
                columns: table => new
                {
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifierId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementProposalVerifiers", x => new { x.SettlementProposalId, x.VerifierId });
                    table.ForeignKey(
                        name: "FK_SettlementProposalVerifiers_AspNetUsers_VerifierId",
                        column: x => x.VerifierId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettlementProposalVerifiers_SettlementProposals_SettlementP~",
                        column: x => x.SettlementProposalId,
                        principalSchema: "truquest",
                        principalTable: "SettlementProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentPollVotes",
                schema: "truquest",
                columns: table => new
                {
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<string>(type: "text", nullable: false),
                    CastedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    VoterSignature = table.Column<string>(type: "text", nullable: false),
                    IpfsCid = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentPollVotes", x => new { x.SettlementProposalId, x.VoterId });
                    table.ForeignKey(
                        name: "FK_AssessmentPollVotes_SettlementProposalVerifiers_SettlementP~",
                        columns: x => new { x.SettlementProposalId, x.VoterId },
                        principalSchema: "truquest",
                        principalTable: "SettlementProposalVerifiers",
                        principalColumns: new[] { "SettlementProposalId", "VerifierId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposalVerifiers_VerifierId",
                schema: "truquest",
                table: "SettlementProposalVerifiers",
                column: "VerifierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcceptancePollVotes",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "AssessmentPollVotes",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "SettlementProposalVerifiers",
                schema: "truquest");

            migrationBuilder.CreateTable(
                name: "Votes",
                schema: "truquest",
                columns: table => new
                {
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<string>(type: "text", nullable: false),
                    CastedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    IpfsCid = table.Column<string>(type: "text", nullable: false),
                    PollType = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    VoterSignature = table.Column<string>(type: "text", nullable: false)
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
    }
}
