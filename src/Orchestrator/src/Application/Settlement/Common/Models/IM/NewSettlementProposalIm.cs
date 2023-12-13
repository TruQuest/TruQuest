using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using Application.Common.Interfaces;

namespace Application.Settlement.Common.Models.IM;

public class NewSettlementProposalIm : IManuallyBoundIm
{
    public Guid ThingId { get; set; }
    public string Title { get; set; }
    public VerdictIm Verdict { get; set; }
    public string Details { get; set; }
    public string? ImagePath { get; set; }
    public string? CroppedImagePath { get; set; }
    public IEnumerable<SettlementProposalEvidenceIm> Evidence { get; set; }

    public string? ImageIpfsCid { get; set; }
    public string? CroppedImageIpfsCid { get; set; }

    public bool BindFrom(FormCollection form)
    {
        if (!(
            form.TryGetValue("thingId", out var value) &&
            !StringValues.IsNullOrEmpty(value) &&
            Guid.TryParse(value, out var thingId)
        ))
        {
            return false;
        }
        ThingId = thingId;

        if (!form.TryGetValue("title", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        Title = value!;

        if (!(
            form.TryGetValue("verdict", out value) &&
            !StringValues.IsNullOrEmpty(value) &&
            int.TryParse(value, out int verdict) &&
            Enum.IsDefined<VerdictIm>((VerdictIm)verdict)
        ))
        {
            return false;
        }
        Verdict = (VerdictIm)verdict;

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
        Evidence = valueSplit.Select(url => new SettlementProposalEvidenceIm { Url = url });

        return true;
    }
}
