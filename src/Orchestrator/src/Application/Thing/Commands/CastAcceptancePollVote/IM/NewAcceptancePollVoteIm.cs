using System.Globalization;

using FluentValidation;

namespace Application.Thing.Commands.CastAcceptancePollVote;

public class NewAcceptancePollVoteIm
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
