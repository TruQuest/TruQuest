using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddWalletAddressToUserEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents");

            migrationBuilder.DropColumn(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "CastedAssessmentPollVoteEvents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
