using Application.Common.Attributes;

namespace Application.Thing.Commands.SubmitNewThing;

public class EvidenceIm
{
    [WebPageUrl(KeepOriginUrl = true)]
    public string Url { get; set; }
}