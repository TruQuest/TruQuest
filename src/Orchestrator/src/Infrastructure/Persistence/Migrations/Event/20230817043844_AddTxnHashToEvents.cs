using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddTxnHashToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "IX_ActionableThingRelatedEvents_ThingId_Type",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents");

            migrationBuilder.AddColumn<long>(
                name: "Nonce",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TxnHash",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserData",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TxnHash",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TxnHash",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TxnHash",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TxnHash",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ThingAssessmentVerifierLotterySpotClaimedEvents_TxnHash",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedThingSubmissionVerifierLotteryEvents_TxnHash",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedThingAssessmentVerifierLotteryEvents_TxnHash",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedAssessmentPollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionableThingRelatedEvents_TxnHash",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents",
                column: "TxnHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThingAssessmentVerifierLotterySpotClaimedEvents_TxnHash",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropIndex(
                name: "IX_JoinedThingSubmissionVerifierLotteryEvents_TxnHash",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents");

            migrationBuilder.DropIndex(
                name: "IX_JoinedThingAssessmentVerifierLotteryEvents_TxnHash",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropIndex(
                name: "IX_CastedAssessmentPollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents");

            migrationBuilder.DropIndex(
                name: "IX_ActionableThingRelatedEvents_TxnHash",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents");

            migrationBuilder.DropColumn(
                name: "Nonce",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "TxnHash",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "UserData",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "TxnHash",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "TxnHash",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "TxnHash",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents");

            migrationBuilder.DropColumn(
                name: "TxnHash",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents");

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
                name: "IX_ActionableThingRelatedEvents_ThingId_Type",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents",
                columns: new[] { "ThingId", "Type" },
                unique: true);
        }
    }
}
