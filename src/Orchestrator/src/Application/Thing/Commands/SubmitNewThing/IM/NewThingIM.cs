using Application.Common.Attributes;
using Application.Common.Models.IM;

namespace Application.Thing.Commands.SubmitNewThing;

public class NewThingIM
{
    public Guid SubjectId { get; set; }
    public string Title { get; set; }
    public string Details { get; set; }
    [ImageURL]
    public string ImageURL { get; set; } = string.Empty;
    public IEnumerable<EvidenceIM> Evidence { get; set; }
    public IEnumerable<TagIM> Tags { get; set; } = new List<TagIM>();
}