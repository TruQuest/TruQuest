using Application.Thing.Common.Models.IM;

namespace Application.Common.Messages.Responses;

public class ArchiveThingAttachmentsSuccessResult
{
    public string SubmitterId { get; set; }
    public Guid ThingId { get; set; }
    public NewThingIm Input { get; set; }
}