using Domain.Aggregates;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddWhitelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:truquest.settlement_proposal_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,accepted")
                .Annotation("Npgsql:Enum:truquest.subject_type", "person,organization")
                .Annotation("Npgsql:Enum:truquest.task_type", "close_thing_validation_verifier_lottery,close_thing_validation_poll,close_settlement_proposal_assessment_verifier_lottery,close_settlement_proposal_assessment_poll")
                .Annotation("Npgsql:Enum:truquest.thing_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,awaiting_settlement,settled")
                .Annotation("Npgsql:Enum:truquest.verdict", "delivered,guess_it_counts,aint_good_enough,motion_not_action,no_effort_whatsoever,as_good_as_malicious_intent")
                .Annotation("Npgsql:Enum:truquest.watched_item_type", "subject,thing,settlement_proposal")
                .Annotation("Npgsql:Enum:truquest.whitelist_entry_type", "email,signer_address")
                .OldAnnotation("Npgsql:Enum:truquest.settlement_proposal_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,accepted")
                .OldAnnotation("Npgsql:Enum:truquest.subject_type", "person,organization")
                .OldAnnotation("Npgsql:Enum:truquest.task_type", "close_thing_validation_verifier_lottery,close_thing_validation_poll,close_settlement_proposal_assessment_verifier_lottery,close_settlement_proposal_assessment_poll")
                .OldAnnotation("Npgsql:Enum:truquest.thing_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,awaiting_settlement,settled")
                .OldAnnotation("Npgsql:Enum:truquest.verdict", "delivered,guess_it_counts,aint_good_enough,motion_not_action,no_effort_whatsoever,as_good_as_malicious_intent")
                .OldAnnotation("Npgsql:Enum:truquest.watched_item_type", "subject,thing,settlement_proposal");

            migrationBuilder.CreateTable(
                name: "Whitelist",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Type = table.Column<WhitelistEntryType>(type: "truquest.whitelist_entry_type", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Whitelist", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Whitelist",
                schema: "truquest");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:truquest.settlement_proposal_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,accepted")
                .Annotation("Npgsql:Enum:truquest.subject_type", "person,organization")
                .Annotation("Npgsql:Enum:truquest.task_type", "close_thing_validation_verifier_lottery,close_thing_validation_poll,close_settlement_proposal_assessment_verifier_lottery,close_settlement_proposal_assessment_poll")
                .Annotation("Npgsql:Enum:truquest.thing_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,awaiting_settlement,settled")
                .Annotation("Npgsql:Enum:truquest.verdict", "delivered,guess_it_counts,aint_good_enough,motion_not_action,no_effort_whatsoever,as_good_as_malicious_intent")
                .Annotation("Npgsql:Enum:truquest.watched_item_type", "subject,thing,settlement_proposal")
                .OldAnnotation("Npgsql:Enum:truquest.settlement_proposal_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,accepted")
                .OldAnnotation("Npgsql:Enum:truquest.subject_type", "person,organization")
                .OldAnnotation("Npgsql:Enum:truquest.task_type", "close_thing_validation_verifier_lottery,close_thing_validation_poll,close_settlement_proposal_assessment_verifier_lottery,close_settlement_proposal_assessment_poll")
                .OldAnnotation("Npgsql:Enum:truquest.thing_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,awaiting_settlement,settled")
                .OldAnnotation("Npgsql:Enum:truquest.verdict", "delivered,guess_it_counts,aint_good_enough,motion_not_action,no_effort_whatsoever,as_good_as_malicious_intent")
                .OldAnnotation("Npgsql:Enum:truquest.watched_item_type", "subject,thing,settlement_proposal")
                .OldAnnotation("Npgsql:Enum:truquest.whitelist_entry_type", "email,signer_address");
        }
    }
}
