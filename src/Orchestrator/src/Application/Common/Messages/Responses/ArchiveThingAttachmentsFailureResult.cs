namespace Application.Common.Messages.Responses;

public class ArchiveThingAttachmentsFailureResult
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required string ErrorMessage { get; init; }
}
