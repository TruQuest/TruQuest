using Domain.Base;

namespace Domain.Aggregates;

public class SettlementProposal : Entity, IAggregateRoot
{
    public Guid Id { get; }
    public Guid ThingId { get; }
    public SettlementProposalState State { get; private set; }
    public long? SubmittedAt { get; private set; }
    public string Title { get; }
    public Verdict Verdict { get; }
    public string Details { get; }
    public string? ImageIpfsCid { get; }
    public string? CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }
    public string? VoteAggIpfsCid { get; private set; }
    public long? AssessmentPronouncedAt { get; private set; }

    private List<SupportingEvidence> _evidence = new();
    public IReadOnlyList<SupportingEvidence> Evidence => _evidence;

    private List<SettlementProposalVerifier> _verifiers = new();
    public IReadOnlyList<SettlementProposalVerifier> Verifiers => _verifiers;

    public SettlementProposal(
        Guid id, Guid thingId, string title, Verdict verdict, string details,
        string? imageIpfsCid, string? croppedImageIpfsCid, string submitterId
    )
    {
        Id = id;
        ThingId = thingId;
        State = SettlementProposalState.Draft;
        Title = title;
        Verdict = verdict;
        Details = details;
        ImageIpfsCid = imageIpfsCid;
        CroppedImageIpfsCid = croppedImageIpfsCid;
        SubmitterId = submitterId;
    }

    public void AddEvidence(IEnumerable<SupportingEvidence> evidence)
    {
        _evidence.AddRange(evidence);
    }

    public void SetState(SettlementProposalState state)
    {
        State = state;
        if (State == SettlementProposalState.AwaitingFunding)
        {
            SubmittedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        else if (State is
            SettlementProposalState.ConsensusNotReached or
            SettlementProposalState.Declined or
            SettlementProposalState.Accepted
        )
        {
            AssessmentPronouncedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public void AddVerifiers(IEnumerable<string> verifierIds)
    {
        _verifiers.AddRange(verifierIds.Select(id => new SettlementProposalVerifier(id)));
    }

    public void SetVoteAggIpfsCid(string voteAggIpfsCid)
    {
        VoteAggIpfsCid = voteAggIpfsCid;
    }
}