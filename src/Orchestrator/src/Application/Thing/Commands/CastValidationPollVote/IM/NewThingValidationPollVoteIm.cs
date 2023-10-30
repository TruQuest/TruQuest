using System.Globalization;

using FluentValidation;

namespace Application.Thing.Commands.CastValidationPollVote;

public class NewThingValidationPollVoteIm
{
    public Guid? ThingId { get; set; }
    public required string CastedAt { get; init; }
    public required DecisionIm Decision { get; init; }
    public required string Reason { get; init; }

    public string ToMessageForSigning() =>
        $"Promise Id: {ThingId}\n" +
        $"Casted At: {CastedAt}\n" +
        $"Decision: {Decision.GetString()}\n" +
        $"Reason: {(Reason.Length != 0 ? Reason : "(Not Specified)")}";

    public static NewThingValidationPollVoteIm FromMessageForSigning(string message)
    {
        var messageSplit = message.Split('\n');
        return new()
        {
            ThingId = Guid.Parse(messageSplit[0].Split(' ').Last()),
            CastedAt = messageSplit[1].Split(' ', 3).Last(),
            Decision = DecisionImExtension.FromString(messageSplit[2].Split(' ', 2).Last()),
            Reason = messageSplit[3].Split(' ', 2).Last()
        };
    }
}

internal class NewThingValidationPollVoteImValidator : AbstractValidator<NewThingValidationPollVoteIm>
{
    public NewThingValidationPollVoteImValidator()
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
