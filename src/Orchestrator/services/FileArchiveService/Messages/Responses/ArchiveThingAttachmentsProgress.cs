namespace Messages.Responses;

internal class ArchiveThingAttachmentsProgress
{
    public required string SubmitterId { get; init; }
    public Guid ThingId { get; init; }
    public int Percent { get; init; }
}