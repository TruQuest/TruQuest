using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddL1BlockNumberToJoinedLottery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "L1BlockNumber",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "L1BlockNumber",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents");
        }
    }
}
