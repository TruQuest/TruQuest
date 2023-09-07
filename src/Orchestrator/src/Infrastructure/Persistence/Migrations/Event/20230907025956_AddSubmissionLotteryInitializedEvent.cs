using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.Event
{
    /// <inheritdoc />
    public partial class AddSubmissionLotteryInitializedEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserData",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ThingSubmissionVerifierLotteryInitializedEvents",
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
                    table.PrimaryKey("PK_ThingSubmissionVerifierLotteryInitializedEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThingSubmissionVerifierLotteryInitializedEvents_TxnHash",
                schema: "truquest_events",
                table: "ThingSubmissionVerifierLotteryInitializedEvents",
                column: "TxnHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThingSubmissionVerifierLotteryInitializedEvents",
                schema: "truquest_events");

            migrationBuilder.AlterColumn<string>(
                name: "UserData",
                schema: "truquest_events",
                table: "JoinedThingSubmissionVerifierLotteryEvents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
