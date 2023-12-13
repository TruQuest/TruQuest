using System.Globalization;

using FluentValidation;

namespace Application.Settlement.Commands.CastAssessmentPollVote;

public class NewSettlementProposalAssessmentPollVoteIm
{
    public required Guid ThingId { get; init; }
    public Guid? SettlementProposalId { get; set; }
    public required string CastedAt { get; init; }
    public required DecisionIm Decision { get; init; }
    public required string Reason { get; init; }

    public string ToMessageForSigning() =>
        $"Promise Id: {ThingId}\n" +
        $"Settlement Proposal Id: {SettlementProposalId}\n" +
        $"Casted At: {CastedAt}\n" +
        $"Decision: {Decision.GetString()}\n" +
        $"Reason: {(Reason.Length != 0 ? Reason : "(Not Specified)")}";
}

internal class NewSettlementProposalAssessmentPollVoteImValidator : AbstractValidator<NewSettlementProposalAssessmentPollVoteIm>
{
    public NewSettlementProposalAssessmentPollVoteImValidator()
    {
        RuleFor(v => v.ThingId).NotEmpty();
        RuleFor(v => v.SettlementProposalId).Must(
            proposalId => proposalId != null && proposalId.Value != Guid.Empty
        );
        RuleFor(v => v.CastedAt).Must(ts => DateTimeOffset.TryParseExact(
            ts, "yyyy-MM-dd HH:mm:sszzz", null, DateTimeStyles.None, out _
        ));
        RuleFor(v => v.Decision).IsInEnum();
    }
}
