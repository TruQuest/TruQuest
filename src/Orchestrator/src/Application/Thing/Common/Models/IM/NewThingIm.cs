using Application.Common.Models.IM;

namespace Application.Thing.Common.Models.IM;

public class NewThingIm
{
    public Guid SubjectId { get; set; }
    public string Title { get; set; }
    public string Details { get; set; }
    public string? ImageUrl { get; set; }
    public IEnumerable<EvidenceIm> Evidence { get; set; }
    public IEnumerable<TagIm> Tags { get; set; } = new List<TagIm>();

    public string? ImageIpfsCid { get; set; }
}