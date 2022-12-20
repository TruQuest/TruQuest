using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class ThingAssessmentEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingIdHash = table.Column<string>(type: "text", nullable: false),
                    SettlementProposalIdHash = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Nonce = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinedThingAssessmentVerifierLotteryEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreJoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingIdHash = table.Column<string>(type: "text", nullable: false),
                    SettlementProposalIdHash = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DataHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreJoinedThingAssessmentVerifierLotteryEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingIdHash = table.Column<string>(type: "text", nullable: false),
                    SettlementProposalIdHash = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingAssessmentVerifierLotterySpotClaimedEvents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "PreJoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                schema: "truquest_events");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "truquest_events",
                table: "CastedAcceptancePollVoteEvents");
        }
    }
}
