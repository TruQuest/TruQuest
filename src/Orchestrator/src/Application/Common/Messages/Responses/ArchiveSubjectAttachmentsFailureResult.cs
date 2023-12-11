namespace Application.Common.Messages.Responses;

public class ArchiveSubjectAttachmentsFailureResult : BaseResponse
{
    public required string ErrorMessage { get; init; }
}
