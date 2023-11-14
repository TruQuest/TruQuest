using System.Collections.Generic;
using Domain.Aggregates;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations.App
{
    /// <inheritdoc />
    public partial class AddDeadLettersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:truquest.dead_letter_source", "actionable_event_from_kafka,task_system")
                .Annotation("Npgsql:Enum:truquest.dead_letter_state", "unhandled,handled")
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
                .OldAnnotation("Npgsql:Enum:truquest.watched_item_type", "subject,thing,settlement_proposal")
                .OldAnnotation("Npgsql:Enum:truquest.whitelist_entry_type", "email,signer_address");

            migrationBuilder.CreateTable(
                name: "DeadLetters",
                schema: "truquest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Source = table.Column<DeadLetterSource>(type: "truquest.dead_letter_source", nullable: false),
                    ArchivedAt = table.Column<long>(type: "bigint", nullable: false),
                    State = table.Column<DeadLetterState>(type: "truquest.dead_letter_state", nullable: false),
                    Payload = table.Column<IReadOnlyDictionary<string, object>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadLetters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeadLetters",
                schema: "truquest");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:truquest.settlement_proposal_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,accepted")
                .Annotation("Npgsql:Enum:truquest.subject_type", "person,organization")
                .Annotation("Npgsql:Enum:truquest.task_type", "close_thing_validation_verifier_lottery,close_thing_validation_poll,close_settlement_proposal_assessment_verifier_lottery,close_settlement_proposal_assessment_poll")
                .Annotation("Npgsql:Enum:truquest.thing_state", "draft,awaiting_funding,funded_and_verifier_lottery_initiated,verifier_lottery_failed,verifiers_selected_and_poll_initiated,consensus_not_reached,declined,awaiting_settlement,settled")
                .Annotation("Npgsql:Enum:truquest.verdict", "delivered,guess_it_counts,aint_good_enough,motion_not_action,no_effort_whatsoever,as_good_as_malicious_intent")
                .Annotation("Npgsql:Enum:truquest.watched_item_type", "subject,thing,settlement_proposal")
                .Annotation("Npgsql:Enum:truquest.whitelist_entry_type", "email,signer_address")
                .OldAnnotation("Npgsql:Enum:truquest.dead_letter_source", "actionable_event_from_kafka,task_system")
                .OldAnnotation("Npgsql:Enum:truquest.dead_letter_state", "unhandled,handled")
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
