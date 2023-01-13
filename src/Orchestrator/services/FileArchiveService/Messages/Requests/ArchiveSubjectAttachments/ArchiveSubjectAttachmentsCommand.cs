using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Messages.Responses;
using Services;

namespace Messages.Requests;

internal class ArchiveSubjectAttachmentsCommand
{
    public required string SubmitterId { get; init; }
    public required NewSubjectIm Input { get; init; }
}

internal class ArchiveSubjectAttachmentsCommandHandler : IMessageHandler<ArchiveSubjectAttachmentsCommand>
{
    private readonly IFileArchiver _fileArchiver;
    private readonly IResponseDispatcher _responseDispatcher;

    public ArchiveSubjectAttachmentsCommandHandler(
        IFileArchiver fileArchiver,
        IResponseDispatcher responseDispatcher
    )
    {
        _fileArchiver = fileArchiver;
        _responseDispatcher = responseDispatcher;
    }

    public async Task Handle(IMessageContext context, ArchiveSubjectAttachmentsCommand message)
    {
        var error = await _fileArchiver.ArchiveAllAttachments(message.Input);
        object response;
        if (error != null)
        {
            response = new ArchiveSubjectAttachmentsFailureResult
            {
                ErrorMessage = error.ToString()
            };
        }
        else
        {
            response = new ArchiveSubjectAttachmentsSuccessResult
            {
                SubmitterId = message.SubmitterId,
                Input = message.Input
            };
        }

        await _responseDispatcher.ReplyTo(Encoding.UTF8.GetString(context.Headers["requestId"]), response);
    }
}