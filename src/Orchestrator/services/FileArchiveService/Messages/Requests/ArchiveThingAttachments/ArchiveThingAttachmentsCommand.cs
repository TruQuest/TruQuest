using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Services;
using Messages.Responses;

namespace Messages.Requests;

internal class ArchiveThingAttachmentsCommand
{
    public required string SubmitterId { get; init; }
    public required Guid ThingId { get; init; }
    public required NewThingIm Input { get; init; }
}

internal class ArchiveThingAttachmentsCommandHandler : IMessageHandler<ArchiveThingAttachmentsCommand>
{
    private readonly ILogger<ArchiveThingAttachmentsCommandHandler> _logger;
    private readonly IFileArchiver _fileArchiver;
    private readonly IResponseDispatcher _responseDispatcher;

    public ArchiveThingAttachmentsCommandHandler(
        ILogger<ArchiveThingAttachmentsCommandHandler> logger,
        IFileArchiver fileArchiver,
        IResponseDispatcher responseDispatcher
    )
    {
        _logger = logger;
        _fileArchiver = fileArchiver;
        _responseDispatcher = responseDispatcher;
    }

    public async Task Handle(IMessageContext context, ArchiveThingAttachmentsCommand message)
    {
        var requestId = Encoding.UTF8.GetString(context.Headers["trq.requestId"]);

        var progress = new Progress<int>(percent =>
        {
            _logger.LogInformation($"Archive Progress: {percent}%");
            _responseDispatcher.SendSync(
                requestId,
                new ArchiveThingAttachmentsProgress
                {
                    SubmitterId = message.SubmitterId,
                    ThingId = message.ThingId,
                    Percent = percent
                },
                key: message.ThingId.ToString()
            );
        });

        object response;
        var error = await _fileArchiver.ArchiveAllAttachments(message.Input, progress);
        if (error != null)
        {
            response = new ArchiveThingAttachmentsFailureResult
            {
                ErrorMessage = error.ToString()
            };
        }
        else
        {
            response = new ArchiveThingAttachmentsSuccessResult
            {
                SubmitterId = message.SubmitterId,
                ThingId = message.ThingId,
                Input = message.Input
            };
        }

        await _responseDispatcher.Send(requestId, response, key: message.ThingId.ToString());
    }
}
