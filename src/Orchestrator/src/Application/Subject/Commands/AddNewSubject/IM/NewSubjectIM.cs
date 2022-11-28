using Application.Common.Attributes;
using Application.Common.Models.IM;

namespace Application.Subject.Commands.AddNewSubject;

public class NewSubjectIM
{
    public SubjectTypeIM Type { get; set; }
    public string Name { get; set; }
    public string Details { get; set; }
    [ImageURL]
    public string ImageURL { get; set; } = string.Empty;
    public IEnumerable<TagIM> Tags { get; set; } = new List<TagIM>();
}