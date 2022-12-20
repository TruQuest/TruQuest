using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class RemoveIdHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettlementProposalIdHash",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "PreJoinedVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "SettlementProposalIdHash",
                schema: "truquest_events",
                table: "PreJoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "PreJoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "JoinedVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "SettlementProposalIdHash",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents");

            migrationBuilder.DropColumn(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "SettlementProposalId",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ThingId",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ThingId",
                schema: "truquest_events",
                table: "PreJoinedVerifierLotteryEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SettlementProposalId",
                schema: "truquest_events",
                table: "PreJoinedThingAssessmentVerifierLotteryEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ThingId",
                schema: "truquest_events",
                table: "PreJoinedThingAssessmentVerifierLotteryEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ThingId",
                schema: "truquest_events",
                table: "JoinedVerifierLotteryEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SettlementProposalId",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ThingId",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ThingId",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ThingId",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettlementProposalId",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "ThingId",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "ThingId",
                schema: "truquest_events",
                table: "PreJoinedVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "SettlementProposalId",
                schema: "truquest_events",
                table: "PreJoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "ThingId",
                schema: "truquest_events",
                table: "PreJoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "ThingId",
                schema: "truquest_events",
                table: "JoinedVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "SettlementProposalId",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "ThingId",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "ThingId",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents");

            migrationBuilder.DropColumn(
                name: "ThingId",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents");

            migrationBuilder.AddColumn<string>(
                name: "SettlementProposalIdHash",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "PreJoinedVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SettlementProposalIdHash",
                schema: "truquest_events",
                table: "PreJoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "PreJoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "JoinedVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SettlementProposalIdHash",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThingIdHash",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
