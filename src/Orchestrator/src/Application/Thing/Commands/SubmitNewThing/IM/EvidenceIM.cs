using Application.Common.Attributes;

namespace Application.Thing.Commands.SubmitNewThing;

public class EvidenceIM
{
    [WebPageURL(KeepOriginURL = true)]
    public string URL { get; set; }
}