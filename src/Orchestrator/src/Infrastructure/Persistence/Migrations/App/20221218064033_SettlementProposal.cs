using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class SettlementProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettlementProposals",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdHash = table.Column<string>(type: "text", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Verdict = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    SubmitterId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementProposals_AspNetUsers_SubmitterId",
                        column: x => x.SubmitterId,
                        principalSchema: "truquest",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettlementProposals_Things_ThingId",
                        column: x => x.ThingId,
                        principalSchema: "truquest",
                        principalTable: "Things",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportingEvidence",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginUrl = table.Column<string>(type: "text", nullable: false),
                    TruUrl = table.Column<string>(type: "text", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportingEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportingEvidence_SettlementProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalSchema: "truquest",
                        principalTable: "SettlementProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposals_IdHash",
                schema: "truquest",
                table: "SettlementProposals",
                column: "IdHash");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposals_SubmitterId",
                schema: "truquest",
                table: "SettlementProposals",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposals_ThingId",
                schema: "truquest",
                table: "SettlementProposals",
                column: "ThingId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportingEvidence_ProposalId",
                schema: "truquest",
                table: "SupportingEvidence",
                column: "ProposalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportingEvidence",
                schema: "truquest");

            migrationBuilder.DropTable(
                name: "SettlementProposals",
                schema: "truquest");
        }
    }
}
