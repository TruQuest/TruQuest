namespace Application.Thing.Queries.GetThing;

public class ThingEvidenceQm
{
    public Guid Id { get; }
    public string OriginUrl { get; }
    public string IpfsCid { get; }
    public string PreviewImageIpfsCid { get; }

    public override bool Equals(object? obj)
    {
        var other = obj as ThingEvidenceQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
