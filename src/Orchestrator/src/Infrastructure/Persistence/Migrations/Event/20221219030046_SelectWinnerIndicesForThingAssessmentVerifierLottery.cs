using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class SelectWinnerIndicesForThingAssessmentVerifierLottery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceUp("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents.v0.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlResourceDown("Infrastructure.Persistence.Migrations.Event.Scripts.Functions.SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents.SelectWinnerIndicesAccordingToPreJoinedThingAssessmentVerifierLotteryEvents.v0.sql");
        }
    }
}
