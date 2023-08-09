using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddUniqueIndicesToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ThingAssessmentVerifierLotterySpotClaimedEvents_SettlementP~",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                columns: new[] { "SettlementProposalId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedThingSubmissionVerifierLotteryEvents_ThingId_UserId",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
                columns: new[] { "ThingId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedThingAssessmentVerifierLotteryEvents_SettlementPropos~",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                columns: new[] { "SettlementProposalId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedAssessmentPollVoteEvents_SettlementProposalId_UserId_~",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents",
                columns: new[] { "SettlementProposalId", "UserId", "BlockNumber", "TxnIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedAcceptancePollVoteEvents_ThingId_UserId_BlockNumber_T~",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                columns: new[] { "ThingId", "UserId", "BlockNumber", "TxnIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionableThingRelatedEvents_ThingId_Type",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents",
                columns: new[] { "ThingId", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThingAssessmentVerifierLotterySpotClaimedEvents_SettlementP~",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropIndex(
                name: "IX_JoinedThingSubmissionVerifierLotteryEvents_ThingId_UserId",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents");

            migrationBuilder.DropIndex(
                name: "IX_JoinedThingAssessmentVerifierLotteryEvents_SettlementPropos~",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropIndex(
                name: "IX_CastedAssessmentPollVoteEvents_SettlementProposalId_UserId_~",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents");

            migrationBuilder.DropIndex(
                name: "IX_CastedAcceptancePollVoteEvents_ThingId_UserId_BlockNumber_T~",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents");

            migrationBuilder.DropIndex(
                name: "IX_ActionableThingRelatedEvents_ThingId_Type",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents");
        }
    }
}
