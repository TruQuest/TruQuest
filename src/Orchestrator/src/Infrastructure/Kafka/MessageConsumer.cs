using System.Text;
using System.Reflection;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using MediatR;
using KafkaFlow;

using Application.Common.Interfaces;
using Application.Common.Messages.Responses;
using Application.Thing.Events.AttachmentsArchivingProgress;
using Application.Thing.Events.AttachmentsArchivingCompleted;

namespace Infrastructure.Kafka;

internal class MessageConsumer : IMessageMiddleware
{
    private readonly ILogger<MessageConsumer> _logger;
    private readonly IRequestDispatcher _requestDispatcher;
    private readonly IPublisher _mediator;

    private readonly Assembly _responseMessagesAssembly;
    private readonly string _responseMessagesNamespace;
    private readonly JsonSerializerOptions _options;

    public MessageConsumer(
        ILogger<MessageConsumer> logger,
        IRequestDispatcher requestDispatcher,
        IPublisher mediator
    )
    {
        _logger = logger;
        _requestDispatcher = requestDispatcher;
        _mediator = mediator;

        _responseMessagesAssembly = Assembly.GetAssembly(typeof(IRequestDispatcher))!;
        _responseMessagesNamespace = "Application.Common.Messages.Responses.";
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var responseType = _responseMessagesAssembly.GetType(
            _responseMessagesNamespace + Encoding.UTF8.GetString(context.Headers["responseType"])
        )!;
        var messageBytes = (byte[])context.Message.Value;
        var message = JsonSerializer.Deserialize(messageBytes, responseType, _options)!;

        var requestId = Encoding.UTF8.GetString(context.Headers["requestId"]);
        if (requestId == Guid.Empty.ToString())
        {
            if (message is ArchiveThingAttachmentsProgress progressResult)
            {
                await _mediator.Publish(new AttachmentsArchivingProgressEvent
                {
                    SubmitterId = progressResult.SubmitterId,
                    ThingId = progressResult.ThingId,
                    Percent = progressResult.Percent
                });
            }
            else if (message is ArchiveThingAttachmentsSuccessResult successResult)
            {
                await _mediator.Publish(new AttachmentsArchivingCompletedEvent
                {
                    SubmitterId = successResult.SubmitterId,
                    ThingId = successResult.ThingId,
                    Input = successResult.Input
                });
            }
        }
        else
        {
            _requestDispatcher.SetResponseFor(requestId, message);
        }

        await next(context);
    }
}