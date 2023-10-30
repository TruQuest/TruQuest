using Domain.Base;

namespace Domain.Aggregates;

public class SettlementProposalAssessmentPollVote : Entity, IAggregateRoot
{
    public enum VoteDecision
    {
        SoftDecline,
        HardDecline,
        Accept,
    }

    public Guid SettlementProposalId { get; }
    public string VoterId { get; }
    public string VoterWalletAddress { get; }
    public long CastedAtMs { get; }
    public VoteDecision Decision { get; }
    public string? Reason { get; }
    public string VoterSignature { get; }
    public string IpfsCid { get; }

    public SettlementProposalAssessmentPollVote(
        Guid settlementProposalId, string voterId, string voterWalletAddress, long castedAtMs,
        VoteDecision decision, string? reason, string voterSignature, string ipfsCid
    )
    {
        SettlementProposalId = settlementProposalId;
        VoterId = voterId;
        VoterWalletAddress = voterWalletAddress;
        CastedAtMs = castedAtMs;
        Decision = decision;
        Reason = reason;
        VoterSignature = voterSignature;
        IpfsCid = ipfsCid;
    }
}

public static class SettlementProposalAssessmentPollVoteDecisionExtension
{
    public static string GetString(this SettlementProposalAssessmentPollVote.VoteDecision decision)
    {
        switch (decision)
        {
            case SettlementProposalAssessmentPollVote.VoteDecision.SoftDecline:
                return "Soft decline";
            case SettlementProposalAssessmentPollVote.VoteDecision.HardDecline:
                return "Hard decline";
            case SettlementProposalAssessmentPollVote.VoteDecision.Accept:
                return "Accept";
        }

        throw new InvalidOperationException();
    }
}
