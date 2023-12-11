namespace Messages.Responses;

internal class ArchiveSubjectAttachmentsFailureResult : BaseResponse
{
    public required string ErrorMessage { get; init; }
}
