using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentValidation;

namespace Application.Settlement.Commands.CastAssessmentPollVote;

public class NewAssessmentPollVoteIm
{
    public required Guid ThingId { get; init; }
    public Guid? SettlementProposalId { get; set; }
    public required string CastedAt { get; init; }
    [JsonConverter(typeof(DecisionImConverter))]
    public required DecisionIm Decision { get; init; }
    public required string Reason { get; init; }
}

internal class NewAssessmentPollVoteImValidator : AbstractValidator<NewAssessmentPollVoteIm>
{
    public NewAssessmentPollVoteImValidator()
    {
        RuleFor(v => v.ThingId).NotEmpty();
        RuleFor(v => v.SettlementProposalId).Must(
            proposalId => proposalId != null && proposalId.Value != Guid.Empty
        );
        RuleFor(v => v.CastedAt).Must(ts => DateTimeOffset.TryParseExact(
            ts, "yyyy-MM-dd HH:mm:sszzz", null, DateTimeStyles.None, out _
        ));
        RuleFor(v => v.Decision).IsInEnum();
        RuleFor(v => v.Reason).NotEmpty();
    }
}

public class DecisionImConverter : JsonConverter<DecisionIm>
{
    public override DecisionIm Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options
    ) => (DecisionIm)reader.GetInt32();

    public override void Write(
        Utf8JsonWriter writer, DecisionIm value, JsonSerializerOptions options
    ) => writer.WriteStringValue(value.GetString());
}
