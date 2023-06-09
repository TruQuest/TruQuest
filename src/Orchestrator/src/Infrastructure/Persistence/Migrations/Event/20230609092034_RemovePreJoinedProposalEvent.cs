using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class RemovePreJoinedProposalEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreJoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.AddColumn<long>(
                name: "L1BlockNumber",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "Nonce",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<long>(
                name: "L1BlockNumber",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "UserData",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "L1BlockNumber",
                schema: "truquest_events",
                table: "ThingAssessmentVerifierLotterySpotClaimedEvents");

            migrationBuilder.DropColumn(
                name: "L1BlockNumber",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.DropColumn(
                name: "UserData",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents");

            migrationBuilder.AlterColumn<decimal>(
                name: "Nonce",
                schema: "truquest_events",
                table: "JoinedThingAssessmentVerifierLotteryEvents",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PreJoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    DataHash = table.Column<string>(type: "text", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreJoinedThingAssessmentVerifierLotteryEvents", x => x.Id);
                });
        }
    }
}
