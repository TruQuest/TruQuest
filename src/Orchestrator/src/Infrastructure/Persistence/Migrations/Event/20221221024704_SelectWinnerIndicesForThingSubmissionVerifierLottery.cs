using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class SelectWinnerIndicesForThingSubmissionVerifierLottery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.v1.sql");
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.v0.sql");

            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedThingSubmissionVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedThingSubmissionVerifierLotteryEvents.v0.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedThingSubmissionVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedThingSubmissionVerifierLotteryEvents.v0.sql");

            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.v0.sql");
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedVerifierLotteryEvents.v1.sql");
        }
    }
}
