using Application.Thing.Common.Models.IM;

namespace Application.Common.Messages.Responses;

public class ArchiveThingAttachmentsSuccessResult
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required NewThingIm Input { get; init; }
}
