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
                    TxnHash = table.Column<string>(type: "text", nullable: false),
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
                name: "CastedSettlementProposalAssessmentPollVoteEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    TxnHash = table.Column<string>(type: "text", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    WalletAddress = table.Column<string>(type: "text", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    L1BlockNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CastedSettlementProposalAssessmentPollVoteEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CastedThingValidationPollVoteEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    TxnHash = table.Column<string>(type: "text", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    WalletAddress = table.Column<string>(type: "text", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    L1BlockNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CastedThingValidationPollVoteEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    TxnHash = table.Column<string>(type: "text", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    WalletAddress = table.Column<string>(type: "text", nullable: false),
                    L1BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    UserData = table.Column<string>(type: "text", nullable: false),
                    Nonce = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JoinedSettlementProposalAssessmentVerifierLotteryEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    TxnHash = table.Column<string>(type: "text", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    WalletAddress = table.Column<string>(type: "text", nullable: false),
                    L1BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    UserData = table.Column<string>(type: "text", nullable: false),
                    Nonce = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinedSettlementProposalAssessmentVerifierLotteryEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JoinedThingValidationVerifierLotteryEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    TxnHash = table.Column<string>(type: "text", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    WalletAddress = table.Column<string>(type: "text", nullable: false),
                    L1BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    UserData = table.Column<string>(type: "text", nullable: false),
                    Nonce = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinedThingValidationVerifierLotteryEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettlementProposalAssessmentVerifierLotteryInitializedEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    TxnHash = table.Column<string>(type: "text", nullable: false),
                    L1BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataHash = table.Column<string>(type: "text", nullable: false),
                    UserXorDataHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementProposalAssessmentVerifierLotteryInitializedEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThingValidationVerifierLotteryInitializedEvents",
                schema: "truquest_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    TxnIndex = table.Column<int>(type: "integer", nullable: false),
                    TxnHash = table.Column<string>(type: "text", nullable: false),
                    L1BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    ThingId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataHash = table.Column<string>(type: "text", nullable: false),
                    UserXorDataHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingValidationVerifierLotteryInitializedEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionableThingRelatedEvents_TxnHash",
                schema: "truquest_events",
                table: "ActionableThingRelatedEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedSettlementProposalAssessmentPollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedSettlementProposalAssessmentPollVoteEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CastedThingValidationPollVoteEvents_TxnHash",
                schema: "truquest_events",
                table: "CastedThingValidationPollVoteEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent~",
                schema: "truquest_events",
                table: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedSettlementProposalAssessmentVerifierLotteryEvents_Txn~",
                schema: "truquest_events",
                table: "JoinedSettlementProposalAssessmentVerifierLotteryEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinedThingValidationVerifierLotteryEvents_TxnHash",
                schema: "truquest_events",
                table: "JoinedThingValidationVerifierLotteryEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementProposalAssessmentVerifierLotteryInitializedEvent~",
                schema: "truquest_events",
                table: "SettlementProposalAssessmentVerifierLotteryInitializedEvents",
                column: "TxnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThingValidationVerifierLotteryInitializedEvents_TxnHash",
                schema: "truquest_events",
                table: "ThingValidationVerifierLotteryInitializedEvents",
                column: "TxnHash",
                unique: true);
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
                name: "CastedSettlementProposalAssessmentPollVoteEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "CastedThingValidationPollVoteEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "JoinedSettlementProposalAssessmentVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "JoinedThingValidationVerifierLotteryEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "SettlementProposalAssessmentVerifierLotteryInitializedEvents",
                schema: "truquest_events");

            migrationBuilder.DropTable(
                name: "ThingValidationVerifierLotteryInitializedEvents",
                schema: "truquest_events");
        }
    }
}
