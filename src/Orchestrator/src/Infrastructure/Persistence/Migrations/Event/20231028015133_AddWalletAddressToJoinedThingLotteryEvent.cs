using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddWalletAddressToJoinedThingLotteryEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
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
                table: "JoinedThingSubmissionVerifierLotteryEvents");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
