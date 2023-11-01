using Application.Common.Models.QM;

namespace Application.Subject.Common.Models.QM;

public class ThingPreviewQm
{
    public required Guid Id { get; init; }
    public required ThingStateQm State { get; init; }
    public required string Title { get; init; }
    public required string? CroppedImageIpfsCid { get; init; }
    public required long? DisplayedTimestamp { get; init; }
    public required VerdictQm? Verdict { get; init; }
    public HashSet<TagQm> Tags { get; } = new();

    public override bool Equals(object? obj)
    {
        var other = obj as ThingPreviewQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
