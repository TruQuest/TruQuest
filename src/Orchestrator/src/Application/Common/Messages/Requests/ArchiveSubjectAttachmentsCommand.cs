using Application.Subject.Commands.AddNewSubject;

namespace Application.Common.Messages.Requests;

public class ArchiveSubjectAttachmentsCommand
{
    public required string SubmitterId { get; init; }
    public required NewSubjectIm Input { get; init; }
}