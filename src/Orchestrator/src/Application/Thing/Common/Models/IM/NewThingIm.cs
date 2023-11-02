using Microsoft.AspNetCore.Http;

using Application.Common.Interfaces;
using Application.Common.Models.IM;

namespace Application.Thing.Common.Models.IM;

public class NewThingIm : IManuallyBoundIm
{
    public Guid SubjectId { get; set; }
    public string Title { get; set; }
    public string Details { get; set; }
    public string? ImagePath { get; set; }
    public string? CroppedImagePath { get; set; }
    public IEnumerable<ThingEvidenceIm> Evidence { get; set; }
    public IEnumerable<TagIm> Tags { get; set; }

    public string? ImageIpfsCid { get; set; }
    public string? CroppedImageIpfsCid { get; set; }

    public void BindFrom(FormCollection form)
    {
        // @@TODO: Validate.
        SubjectId = Guid.Parse(form["subjectId"]!);
        Title = form["title"]!;
        Details = form["details"]!;
        ImagePath = form["file1"];
        CroppedImagePath = form["file2"];
        Evidence = ((string)form["evidence"]!).Split('|')
            .Select(url => new ThingEvidenceIm { Url = url });
        Tags = ((string)form["tags"]!).Split('|')
            .Select(tagIdStr => new TagIm { Id = int.Parse(tagIdStr) });
    }
}
