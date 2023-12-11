using Application.Common.Monitoring;

namespace Application.Common.Messages.Responses;

public class ArchiveThingAttachmentsFailureResult : BaseResponse
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required string ErrorMessage { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.ThingId, ThingId)
        };
    }
}
