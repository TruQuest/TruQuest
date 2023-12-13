using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

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

    public bool BindFrom(FormCollection form)
    {
        if (!(
            form.TryGetValue("subjectId", out var value) &&
            !StringValues.IsNullOrEmpty(value) &&
            Guid.TryParse(value, out var subjectId)
        ))
        {
            return false;
        }
        SubjectId = subjectId;

        if (!form.TryGetValue("title", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        Title = value!;

        if (!form.TryGetValue("details", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        Details = value!;

        if (!form.TryGetValue("file1", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        ImagePath = value!;

        if (!form.TryGetValue("file2", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        CroppedImagePath = value!;

        string[] valueSplit;
        if (!(
            form.TryGetValue("evidence", out value) &&
            !StringValues.IsNullOrEmpty(value) &&
            (valueSplit = ((string)value!).Split('|')).Length > 0 &&
            valueSplit.All(v =>
                Uri.TryCreate(v, UriKind.Absolute, out var url) &&
                (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps)
            )
        ))
        {
            return false;
        }
        Evidence = valueSplit.Select(url => new ThingEvidenceIm { Url = url });

        if (!(
            form.TryGetValue("tags", out value) &&
            !StringValues.IsNullOrEmpty(value) &&
            (valueSplit = ((string)value!).Split('|')).Length > 0 &&
            valueSplit.All(v => int.TryParse(v, out _))
        ))
        {
            return false;
        }
        Tags = valueSplit.Select(tagId => new TagIm { Id = int.Parse(tagId) });

        return true;
    }
}
