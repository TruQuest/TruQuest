using System.Diagnostics;
using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
        var propagationContext = Propagators.DefaultTextMapPropagator.Extract(
            default,
            context.Headers,
            (headers, key) =>
            {
                if (headers.Any(kv => kv.Key == key))
                {
                    return new[] { Encoding.UTF8.GetString(headers[key]) };
                }
                return Enumerable.Empty<string>();
            });

        Baggage.Current = propagationContext.Baggage;

        object response;
        using (var span = Telemetry.StartActivity(
            "requests process",
            ActivityKind.Server,
            parentContext: propagationContext.ActivityContext
        ))
        {
            var error = await _fileArchiver.ArchiveAllAttachments(message.Input);
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
        }

        await _responseDispatcher.ReplyTo(
            Encoding.UTF8.GetString(context.Headers["requestId"]),
            response,
            parentContext: propagationContext.ActivityContext
        );
    }
}