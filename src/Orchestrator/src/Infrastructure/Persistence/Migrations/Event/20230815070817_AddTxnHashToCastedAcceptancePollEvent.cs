using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddTxnHashToCastedAcceptancePollEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CastedAcceptancePollVoteEvents_ThingId_UserId_BlockNumber_T~",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents");

            migrationBuilder.AddColumn<string>(
                name: "TxnHash",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CastedAcceptancePollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                column: "TxnHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CastedAcceptancePollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents");

            migrationBuilder.DropColumn(
                name: "TxnHash",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents");

            migrationBuilder.CreateIndex(
                name: "IX_CastedAcceptancePollVoteEvents_ThingId_UserId_BlockNumber_T~",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                columns: new[] { "ThingId", "UserId", "BlockNumber", "TxnIndex" },
                unique: true);
        }
    }
}
