namespace Application.Thing.Queries.GetThing;

public class EvidenceQm
{
    public Guid Id { get; }
    public string OriginUrl { get; }
    public string IpfsCid { get; }
    public string PreviewImageIpfsCid { get; }

    public override bool Equals(object? obj)
    {
        var other = obj as EvidenceQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}