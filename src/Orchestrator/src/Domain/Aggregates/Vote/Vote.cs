using Domain.Base;

namespace Domain.Aggregates;

public class Vote : Entity, IAggregateRoot
{
    public Guid ThingId { get; }
    public string VoterId { get; }
    public PollType PollType { get; }
    public long CastedAtMs { get; }
    public Decision Decision { get; }
    public string? Reason { get; }
    public string VoterSignature { get; }
    public string IpfsCid { get; }

    public Vote(
        Guid thingId, string voterId, PollType pollType, long castedAtMs,
        Decision decision, string? reason, string voterSignature, string ipfsCid)
    {
        ThingId = thingId;
        VoterId = voterId;
        PollType = pollType;
        CastedAtMs = castedAtMs;
        Decision = decision;
        Reason = reason;
        VoterSignature = voterSignature;
        IpfsCid = ipfsCid;
    }
}