using Domain.Base;

namespace Domain.Aggregates;

public class Thing : Entity, IAggregateRoot
{
    public Guid? Id { get; private set; }
    public string? IdHash { get; private set; }
    public ThingState State { get; private set; }
    public string Title { get; }
    public string Details { get; }
    public string? ImageUrl { get; }
    public string SubmitterId { get; }
    public Guid SubjectId { get; }

    private List<Evidence> _evidence = new();
    public IReadOnlyList<Evidence> Evidence => _evidence;

    private List<ThingAttachedTag> _tags = new();
    public IReadOnlyList<ThingAttachedTag> Tags => _tags;

    private List<ThingVerifier> _verifiers = new();
    public IReadOnlyList<ThingVerifier> Verifiers => _verifiers;

    public Thing(string title, string details, string? imageUrl, string submitterId, Guid subjectId)
    {
        State = ThingState.AwaitingFunding;
        Title = title;
        Details = details;
        ImageUrl = imageUrl;
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
}