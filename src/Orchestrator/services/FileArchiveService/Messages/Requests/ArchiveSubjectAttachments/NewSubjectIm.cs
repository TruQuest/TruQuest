using Attributes;
using Common.Models.IM;

namespace Messages.Requests;

internal class NewSubjectIm
{
    public required SubjectTypeIm Type { get; init; }
    public required string Name { get; init; }
    public required string Details { get; init; }
    [ImagePath(BackingField = nameof(ImageIpfsCid))]
    public required string ImagePath { get; init; }
    [ImagePath(BackingField = nameof(CroppedImageIpfsCid))]
    public required string CroppedImagePath { get; init; }
    public required IEnumerable<TagIm> Tags { get; init; }

    [BackingField]
    public required string? ImageIpfsCid { get; set; }
    [BackingField]
    public required string? CroppedImageIpfsCid { get; set; }
}