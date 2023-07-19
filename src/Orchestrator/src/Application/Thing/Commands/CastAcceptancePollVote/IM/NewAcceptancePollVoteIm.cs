using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentValidation;

namespace Application.Thing.Commands.CastAcceptancePollVote;

public class NewAcceptancePollVoteIm
{
    public Guid? ThingId { get; set; }
    public required string CastedAt { get; init; }
    [JsonConverter(typeof(DecisionImConverter))]
    public required DecisionIm Decision { get; init; }
    public required string Reason { get; init; }
}

internal class NewAcceptancePollVoteImValidator : AbstractValidator<NewAcceptancePollVoteIm>
{
    public NewAcceptancePollVoteImValidator()
    {
        RuleFor(v => v.ThingId).Must(
            thingId => thingId != null && thingId.Value != Guid.Empty
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
