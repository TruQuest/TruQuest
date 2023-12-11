using Common.Monitoring;
using Messages.Requests;

namespace Messages.Responses;

internal class ArchiveThingAttachmentsSuccessResult : BaseResponse
{
    public required string SubmitterId { get; init; }
    public Guid ThingId { get; init; }
    public required NewThingIm Input { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.SubjectId, Input.SubjectId),
            (ActivityTags.ThingId, ThingId)
        };
    }
}
