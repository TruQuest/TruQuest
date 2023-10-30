using Application.Common.Models.IM;

namespace Application.Thing.Common.Models.IM;

public class NewThingIm
{
    public required Guid SubjectId { get; init; }
    public required string Title { get; init; }
    public required string Details { get; init; }
    public required string? ImagePath { get; init; }
    public required string? CroppedImagePath { get; init; }
    public required IEnumerable<ThingEvidenceIm> Evidence { get; init; }
    public required IEnumerable<TagIm> Tags { get; init; }

    public string? ImageIpfsCid { get; init; }
    public string? CroppedImageIpfsCid { get; init; }
}
