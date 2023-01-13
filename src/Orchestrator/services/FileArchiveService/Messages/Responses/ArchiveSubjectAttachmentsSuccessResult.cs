using Messages.Requests;

namespace Messages.Responses;

internal class ArchiveSubjectAttachmentsSuccessResult
{
    public required string SubmitterId { get; init; }
    public required NewSubjectIm Input { get; init; }
}