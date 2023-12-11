using Common.Monitoring;
using Messages.Requests;

namespace Messages.Responses;

internal class ArchiveSubjectAttachmentsSuccessResult : BaseResponse
{
    public required string SubmitterId { get; init; }
    public required NewSubjectIm Input { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId)
        };
    }
}
