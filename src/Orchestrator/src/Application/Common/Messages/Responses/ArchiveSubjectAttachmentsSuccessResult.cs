using Application.Subject.Commands.AddNewSubject;

namespace Application.Common.Messages.Responses;

public class ArchiveSubjectAttachmentsSuccessResult
{
    public required string SubmitterId { get; init; }
    public required NewSubjectIm Input { get; init; }
}