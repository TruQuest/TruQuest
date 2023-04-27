using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "truquest_events");

            migrationBuilder.CreateTable(
                name: "ActionableThingRelatedEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<IReadOnlyDictionary<string, object>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionableThingRelatedEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlockProcessedEvent",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockProcessedEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CastedAcceptancePollVoteEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CastedAcceptancePollVoteEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CastedAssessmentPollVoteEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CastedAssessmentPollVoteEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Nonce = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinedThingAssessmentVerifierLotteryEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JoinedThingSubmissionVerifierLotteryEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Nonce = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinedThingSubmissionVerifierLotteryEvents", x => x.Id);
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
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DataHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreJoinedThingAssessmentVerifierLotteryEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreJoinedThingSubmissionVerifierLotteryEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DataHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreJoinedThingSubmissionVerifierLotteryEvents", x => x.Id);
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
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "ActionableThingRelatedEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "BlockProcessedEvent",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "CastedAcceptancePollVoteEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "CastedAssessmentPollVoteEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "JoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "JoinedThingSubmissionVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "PreJoinedThingAssessmentVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "PreJoinedThingSubmissionVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "ThingAssessmentVerifierLotterySpotClaimedEvents",
                schema: "truquest_events");
        }
    }
}
