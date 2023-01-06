using Application.Thing.Common.Models.IM;

namespace Application.Common.Messages.Requests;

public class ArchiveThingAttachmentsCommand
{
    public required string SubmitterId { get; init; }
    public Guid ThingId { get; init; }
    public required NewThingIm Input { get; init; }
}