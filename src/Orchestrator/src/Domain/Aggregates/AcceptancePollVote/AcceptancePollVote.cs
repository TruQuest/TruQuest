using Domain.Base;

namespace Domain.Aggregates;

public class AcceptancePollVote : Entity, IAggregateRoot
{
    public enum VoteDecision
    {
        SoftDecline,
        HardDecline,
        Accept,
    }

    public Guid ThingId { get; }
    public string VoterId { get; }
    public long CastedAtMs { get; }
    public VoteDecision Decision { get; }
    public string? Reason { get; }
    public string VoterSignature { get; }
    public string IpfsCid { get; }

    public AcceptancePollVote(
        Guid thingId, string voterId, long castedAtMs, VoteDecision decision,
        string? reason, string voterSignature, string ipfsCid
    )
    {
        ThingId = thingId;
        VoterId = voterId;
        CastedAtMs = castedAtMs;
        Decision = decision;
        Reason = reason;
        VoterSignature = voterSignature;
        IpfsCid = ipfsCid;
    }
}

public static class AcceptancePollVoteDecisionExtension
{
    public static string GetString(this AcceptancePollVote.VoteDecision decision)
    {
        switch (decision)
        {
            case AcceptancePollVote.VoteDecision.SoftDecline:
                return "Soft decline";
            case AcceptancePollVote.VoteDecision.HardDecline:
                return "Hard decline";
            case AcceptancePollVote.VoteDecision.Accept:
                return "Accept";
        }

        throw new InvalidOperationException();
    }
}