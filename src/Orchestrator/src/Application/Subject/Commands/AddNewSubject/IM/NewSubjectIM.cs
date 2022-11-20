using Application.Common.Attributes;

namespace Application.Subject.Commands.AddNewSubject;

public class NewSubjectIM
{
    public SubjectTypeIM Type { get; set; }
    public string Name { get; set; }
    public string Details { get; set; }
    [ImageURL]
    public string ImageURL { get; set; } = string.Empty;
    [WebPageURL]
    public string ProfilePageURL { get; set; } = string.Empty;
    public IEnumerable<TagIM> Tags { get; set; } = new List<TagIM>();
}