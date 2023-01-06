using Attributes;

namespace Messages.Requests;

internal class NewThingIm
{
    public Guid SubjectId { get; set; }
    public string Title { get; set; }
    public string Details { get; set; }
    [ImageUrl(BackingField = nameof(ImageIpfsCid))]
    public string? ImageUrl { get; set; }
    public IEnumerable<EvidenceIm> Evidence { get; set; }
    public IEnumerable<TagIm> Tags { get; set; }

    [BackingField]
    public string? ImageIpfsCid { get; set; }
}