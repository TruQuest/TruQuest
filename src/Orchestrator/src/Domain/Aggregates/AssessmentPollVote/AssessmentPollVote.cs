using Domain.Base;

namespace Domain.Aggregates;

public class AssessmentPollVote : Entity, IAggregateRoot
{
    public enum VoteDecision
    {
        SoftDecline,
        HardDecline,
        Accept,
    }

    public Guid SettlementProposalId { get; }
    public string VoterId { get; }
    public long CastedAtMs { get; }
    public VoteDecision Decision { get; }
    public string? Reason { get; }
    public string VoterSignature { get; }
    public string IpfsCid { get; }

    public AssessmentPollVote(
        Guid settlementProposalId, string voterId, long castedAtMs,
        VoteDecision decision, string? reason, string voterSignature, string ipfsCid
    )
    {
        SettlementProposalId = settlementProposalId;
        VoterId = voterId;
        CastedAtMs = castedAtMs;
        Decision = decision;
        Reason = reason;
        VoterSignature = voterSignature;
        IpfsCid = ipfsCid;
    }
}

public static class AssessmentPollVoteDecisionExtension
{
    public static string GetString(this AssessmentPollVote.VoteDecision decision)
    {
        switch (decision)
        {
            case AssessmentPollVote.VoteDecision.SoftDecline:
                return "Soft decline";
            case AssessmentPollVote.VoteDecision.HardDecline:
                return "Hard decline";
            case AssessmentPollVote.VoteDecision.Accept:
                return "Accept";
        }

        throw new InvalidOperationException();
    }
}