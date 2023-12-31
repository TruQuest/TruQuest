using Domain.Base;

namespace Domain.Aggregates;

public class Thing : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public ThingState State { get; private set; }
    public long? SubmittedAt { get; private set; }
    public string Title { get; }
    public string Details { get; }
    public string? ImageIpfsCid { get; }
    public string? CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }
    public Guid SubjectId { get; }
    public string? VoteAggIpfsCid { get; private set; }
    public Guid? AcceptedSettlementProposalId { get; private set; }
    public long? SettledAt { get; private set; }

    // @@??: Do we need smth like SubmissionEvaluatedAt ?

    private List<ThingEvidence> _evidence = new();
    public IReadOnlyList<ThingEvidence> Evidence => _evidence;

    private List<ThingAttachedTag> _tags = new();
    public IReadOnlyList<ThingAttachedTag> Tags => _tags;

    private List<ThingVerifier> _verifiers = new();
    public IReadOnlyList<ThingVerifier> Verifiers => _verifiers;

    private Dictionary<string, string>? _relatedThings;
    public IReadOnlyDictionary<string, string>? RelatedThings => _relatedThings;

    public Thing(
        Guid id, string title, string details, string? imageIpfsCid,
        string? croppedImageIpfsCid, string submitterId, Guid subjectId
    )
    {
        Id = id;
        State = ThingState.Draft;
        Title = title;
        Details = details;
        ImageIpfsCid = imageIpfsCid;
        CroppedImageIpfsCid = croppedImageIpfsCid;
        SubmitterId = submitterId;
        SubjectId = subjectId;
    }

    public void AddEvidence(IEnumerable<ThingEvidence> evidence)
    {
        _evidence.AddRange(evidence);
    }

    public void AddTags(IEnumerable<int> tagIds)
    {
        _tags.AddRange(tagIds.Select(tagId => new ThingAttachedTag(tagId)));
    }

    public void SetState(ThingState state)
    {
        State = state;
        if (State == ThingState.AwaitingFunding)
        {
            SubmittedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        else if (State == ThingState.Settled)
        {
            SettledAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public void AddVerifiers(IEnumerable<string> verifierIds)
    {
        _verifiers.AddRange(verifierIds.Select(id => new ThingVerifier(id)));
    }

    public void SetVoteAggIpfsCid(string voteAggIpfsCid)
    {
        VoteAggIpfsCid = voteAggIpfsCid;
    }

    public void AcceptSettlementProposal(Guid settlementProposalId)
    {
        AcceptedSettlementProposalId = settlementProposalId;
    }

    public void AddRelatedThingAs(Guid thingId, string relation)
    {
        _relatedThings ??= new();
        _relatedThings[relation] = thingId.ToString();
    }
}
