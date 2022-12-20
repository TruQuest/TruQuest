using Domain.Base;

namespace Domain.Aggregates;

public class SettlementProposal : Entity, IAggregateRoot
{
    public Guid? Id { get; private set; }
    public string? IdHash { get; private set; }
    public Guid ThingId { get; }
    public SettlementProposalState State { get; private set; }
    public string Title { get; }
    public Verdict Verdict { get; }
    public string Details { get; }
    public string SubmitterId { get; }

    private List<SupportingEvidence> _evidence = new();
    public IReadOnlyList<SupportingEvidence> Evidence => _evidence;

    public SettlementProposal(Guid thingId, string title, Verdict verdict, string details, string submitterId)
    {
        ThingId = thingId;
        State = SettlementProposalState.AwaitingFunding;
        Title = title;
        Verdict = verdict;
        Details = details;
        SubmitterId = submitterId;
    }

    public void AddEvidence(IEnumerable<SupportingEvidence> evidence)
    {
        _evidence.AddRange(evidence);
    }

    public void SetState(SettlementProposalState state)
    {
        State = state;
    }
}