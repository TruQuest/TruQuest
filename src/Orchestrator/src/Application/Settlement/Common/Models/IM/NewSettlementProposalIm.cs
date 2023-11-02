using Microsoft.AspNetCore.Http;

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

    public void BindFrom(FormCollection form)
    {
        // @@TODO: Validate.
        ThingId = Guid.Parse(form["thingId"]!);
        Title = form["title"]!;
        Verdict = (VerdictIm)int.Parse(form["verdict"]!);
        Details = form["details"]!;
        ImagePath = form["file1"];
        CroppedImagePath = form["file2"];
        Evidence = ((string)form["evidence"]!).Split('|')
            .Select(url => new SettlementProposalEvidenceIm { Url = url });
    }
}
