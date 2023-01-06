using Messages.Requests;

namespace Messages.Responses;

internal class ArchiveThingAttachmentsSuccessResult
{
    public required string SubmitterId { get; init; }
    public Guid ThingId { get; init; }
    public required NewThingIm Input { get; init; }
}