using Application.Common.Attributes;
using Application.Common.Models.IM;

namespace Application.Thing.Commands.SubmitNewThing;

public class NewThingIm
{
    public Guid SubjectId { get; set; }
    public string Title { get; set; }
    public string Details { get; set; }
    [ImageUrl]
    public string ImageUrl { get; set; } = string.Empty;
    public IEnumerable<EvidenceIm> Evidence { get; set; }
    public IEnumerable<TagIm> Tags { get; set; } = new List<TagIm>();
}