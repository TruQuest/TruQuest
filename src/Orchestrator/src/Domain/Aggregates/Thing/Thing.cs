using Domain.Base;

namespace Domain.Aggregates;

public class Thing : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public ThingState State { get; private set; }
    public long SubmittedAt { get; }
    public string Title { get; }
    public string Details { get; }
    public string? ImageIpfsCid { get; }
    public string? CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }
    public Guid SubjectId { get; }
    public string? VoteAggIpfsCid { get; private set; }
    public Guid? AcceptedSettlementProposalId { get; private set; }

    private List<Evidence> _evidence = new();
    public IReadOnlyList<Evidence> Evidence => _evidence;

    private List<ThingAttachedTag> _tags = new();
    public IReadOnlyList<ThingAttachedTag> Tags => _tags;

    private List<ThingVerifier> _verifiers = new();
    public IReadOnlyList<ThingVerifier> Verifiers => _verifiers;

    public Thing(
        Guid id, string title, string details, string? imageIpfsCid,
        string? croppedImageIpfsCid, string submitterId, Guid subjectId
    )
    {
        Id = id;
        State = ThingState.Draft;
        SubmittedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Title = title;
        Details = details;
        ImageIpfsCid = imageIpfsCid;
        CroppedImageIpfsCid = croppedImageIpfsCid;
        SubmitterId = submitterId;
        SubjectId = subjectId;
    }

    public void AddEvidence(IEnumerable<Evidence> evidence)
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
}