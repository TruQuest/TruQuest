using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddLogIndexToUserActionsEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JoinedThingValidationVerifierLotteryEvents_TxnHash",
                schema: "truquest_events",
                table: "JoinedThingValidationVerifierLotteryEvents");

            migrationBuilder.DropIndex(
                name: "IX_JoinedSettlementProposalAssessmentVerifierLotteryEvents_Txn~",
                schema: "truquest_events",
                table: "JoinedSettlementProposalAssessmentVerifierLotteryEvents");

            migrationBuilder.DropIndex(
                name: "IX_ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent~",
                schema: "truquest_events",
                table: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents");

            migrationBuilder.DropIndex(
                name: "IX_CastedThingValidationPollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedThingValidationPollVoteEvents");

            migrationBuilder.DropIndex(
                name: "IX_CastedSettlementProposalAssessmentPollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedSettlementProposalAssessmentPollVoteEvents");

            migrationBuilder.AddColumn<int>(
                name: "LogIndex",
                schema: "truquest_events",
                table: "JoinedThingValidationVerifierLotteryEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LogIndex",
                schema: "truquest_events",
                table: "JoinedSettlementProposalAssessmentVerifierLotteryEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LogIndex",
                schema: "truquest_events",
                table: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LogIndex",
                schema: "truquest_events",
                table: "CastedThingValidationPollVoteEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LogIndex",
                schema: "truquest_events",
                table: "CastedSettlementProposalAssessmentPollVoteEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedThingValidationVerifierLotteryEvents_TxnHash_LogIndex",
                schema: "truquest_events",
                table: "JoinedThingValidationVerifierLotteryEvents",
                columns: new[] { "TxnHash", "LogIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedSettlementProposalAssessmentVerifierLotteryEvents_Txn~",
                schema: "truquest_events",
                table: "JoinedSettlementProposalAssessmentVerifierLotteryEvents",
                columns: new[] { "TxnHash", "LogIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent~",
                schema: "truquest_events",
                table: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents",
                columns: new[] { "TxnHash", "LogIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedThingValidationPollVoteEvents_TxnHash_LogIndex",
                schema: "truquest_events",
                table: "CastedThingValidationPollVoteEvents",
                columns: new[] { "TxnHash", "LogIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedSettlementProposalAssessmentPollVoteEvents_TxnHash_Lo~",
                schema: "truquest_events",
                table: "CastedSettlementProposalAssessmentPollVoteEvents",
                columns: new[] { "TxnHash", "LogIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JoinedThingValidationVerifierLotteryEvents_TxnHash_LogIndex",
                schema: "truquest_events",
                table: "JoinedThingValidationVerifierLotteryEvents");

            migrationBuilder.DropIndex(
                name: "IX_JoinedSettlementProposalAssessmentVerifierLotteryEvents_Txn~",
                schema: "truquest_events",
                table: "JoinedSettlementProposalAssessmentVerifierLotteryEvents");

            migrationBuilder.DropIndex(
                name: "IX_ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent~",
                schema: "truquest_events",
                table: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents");

            migrationBuilder.DropIndex(
                name: "IX_CastedThingValidationPollVoteEvents_TxnHash_LogIndex",
                schema: "truquest_events",
                table: "CastedThingValidationPollVoteEvents");

            migrationBuilder.DropIndex(
                name: "IX_CastedSettlementProposalAssessmentPollVoteEvents_TxnHash_Lo~",
                schema: "truquest_events",
                table: "CastedSettlementProposalAssessmentPollVoteEvents");

            migrationBuilder.DropColumn(
                name: "LogIndex",
                schema: "truquest_events",
                table: "JoinedThingValidationVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "LogIndex",
                schema: "truquest_events",
                table: "JoinedSettlementProposalAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "LogIndex",
                schema: "truquest_events",
                table: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents");

            migrationBuilder.DropColumn(
                name: "LogIndex",
                schema: "truquest_events",
                table: "CastedThingValidationPollVoteEvents");

            migrationBuilder.DropColumn(
                name: "LogIndex",
                schema: "truquest_events",
                table: "CastedSettlementProposalAssessmentPollVoteEvents");

            migrationBuilder.CreateIndex(
                name: "IX_JoinedThingValidationVerifierLotteryEvents_TxnHash",
                schema: "truquest_events",
                table: "JoinedThingValidationVerifierLotteryEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedSettlementProposalAssessmentVerifierLotteryEvents_Txn~",
                schema: "truquest_events",
                table: "JoinedSettlementProposalAssessmentVerifierLotteryEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent~",
                schema: "truquest_events",
                table: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedThingValidationPollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedThingValidationPollVoteEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedSettlementProposalAssessmentPollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedSettlementProposalAssessmentPollVoteEvents",
                column: "TxnHash",
                unique: true);
        }
    }
}
