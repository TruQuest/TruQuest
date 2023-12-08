namespace Application.Common.Messages.Responses;

public class ArchiveThingAttachmentsProgress
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required int Percent { get; init; }
}
