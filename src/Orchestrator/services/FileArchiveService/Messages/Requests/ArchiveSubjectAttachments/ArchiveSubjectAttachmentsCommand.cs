using System.Text;

using KafkaFlow;

using Services;
using Messages.Responses;
using Common.Monitoring;

namespace Messages.Requests;

internal class ArchiveSubjectAttachmentsCommand : BaseRequest
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

internal class ArchiveSubjectAttachmentsCommandHandler : IMessageHandler<ArchiveSubjectAttachmentsCommand>
{
    private readonly ILogger<ArchiveSubjectAttachmentsCommandHandler> _logger;
    private readonly IFileArchiver _fileArchiver;
    private readonly IResponseDispatcher _responseDispatcher;

    public ArchiveSubjectAttachmentsCommandHandler(
        ILogger<ArchiveSubjectAttachmentsCommandHandler> logger,
        IFileArchiver fileArchiver,
        IResponseDispatcher responseDispatcher
    )
    {
        _logger = logger;
        _fileArchiver = fileArchiver;
        _responseDispatcher = responseDispatcher;
    }

    public async Task Handle(IMessageContext context, ArchiveSubjectAttachmentsCommand message)
    {
        var requestId = Encoding.UTF8.GetString(context.Headers["trq.requestId"]);

        object response;
        var error = await _fileArchiver.ArchiveAllAttachments(requestId, message.Input);
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

        await _responseDispatcher.Reply(requestId, response);
    }
}
