using Application.Common.Models.IM;

namespace Application.Subject.Commands.AddNewSubject;

public class NewSubjectIm
{
    public required SubjectTypeIm Type { get; init; }
    public required string Name { get; init; }
    public required string Details { get; init; }
    public required string ImagePath { get; init; }
    public required string CroppedImagePath { get; init; }
    public required IEnumerable<TagIm> Tags { get; init; }

    public string? ImageIpfsCid { get; init; }
    public string? CroppedImageIpfsCid { get; init; }
}