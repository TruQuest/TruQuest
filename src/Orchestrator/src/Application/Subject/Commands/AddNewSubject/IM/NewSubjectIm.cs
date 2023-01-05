using Application.Common.Attributes;
using Application.Common.Models.IM;

namespace Application.Subject.Commands.AddNewSubject;

public class NewSubjectIm
{
    public SubjectTypeIm Type { get; set; }
    public string Name { get; set; }
    public string Details { get; set; }
    [ImageUrl(BackingField = nameof(ImageIpfsCid))]
    public string ImageUrl { get; set; } = string.Empty;
    public IEnumerable<TagIm> Tags { get; set; } = new List<TagIm>();

    internal string? ImageIpfsCid { get; set; }
}