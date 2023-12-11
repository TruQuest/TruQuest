using Common.Monitoring;

namespace Messages.Responses;

internal class ArchiveThingAttachmentsProgress : BaseResponse
{
    public required string SubmitterId { get; init; }
    public Guid ThingId { get; init; }
    public int Percent { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.ThingId, ThingId)
        };
    }
}
