using Domain.Base;

namespace Domain.Aggregates;

public class ThingValidationPollVote : Entity, IAggregateRoot
{
    public enum VoteDecision
    {
        SoftDecline,
        HardDecline,
        Accept,
    }

    public Guid ThingId { get; }
    public string VoterId { get; }
    public string VoterWalletAddress { get; }
    public long CastedAtMs { get; }
    public VoteDecision Decision { get; }
    public string? Reason { get; }
    public string VoterSignature { get; }
    public string IpfsCid { get; }

    public ThingValidationPollVote(
        Guid thingId, string voterId, string voterWalletAddress, long castedAtMs,
        VoteDecision decision, string? reason, string voterSignature, string ipfsCid
    )
    {
        ThingId = thingId;
        VoterId = voterId;
        VoterWalletAddress = voterWalletAddress;
        CastedAtMs = castedAtMs;
        Decision = decision;
        Reason = reason;
        VoterSignature = voterSignature;
        IpfsCid = ipfsCid;
    }
}

public static class ThingValidationPollVoteDecisionExtension
{
    public static string GetString(this ThingValidationPollVote.VoteDecision decision)
    {
        switch (decision)
        {
            case ThingValidationPollVote.VoteDecision.SoftDecline:
                return "Soft decline";
            case ThingValidationPollVote.VoteDecision.HardDecline:
                return "Hard decline";
            case ThingValidationPollVote.VoteDecision.Accept:
                return "Accept";
        }

        throw new InvalidOperationException();
    }

    public static ThingValidationPollVote.VoteDecision FromString(string decision)
    {
        switch (decision)
        {
            case "Soft decline":
                return ThingValidationPollVote.VoteDecision.SoftDecline;
            case "Hard decline":
                return ThingValidationPollVote.VoteDecision.HardDecline;
            case "Accept":
                return ThingValidationPollVote.VoteDecision.Accept;
        }

        throw new InvalidOperationException();
    }
}
