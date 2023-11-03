using System.Text;
using System.Reflection;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using KafkaFlow;

using Application.Common.Interfaces;
using Application.Common.Messages.Responses;
using ThingEvents = Application.Thing.Events;
using SettlementEvents = Application.Settlement.Events;

namespace Infrastructure.Kafka;

internal class MessageConsumer : IMessageMiddleware
{
    private readonly ILogger<MessageConsumer> _logger;
    private readonly IRequestDispatcher _requestDispatcher;
    private readonly PublisherWrapper _mediator;

    private readonly Assembly _responseMessagesAssembly;
    private readonly string _responseMessagesNamespace;
    private readonly JsonSerializerOptions _options;

    public MessageConsumer(
        ILogger<MessageConsumer> logger,
        IRequestDispatcher requestDispatcher,
        PublisherWrapper mediator
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
        // @@TODO: Refactor. Use MessageTypeResolver and MessageSerializer instead of this manual stuff.

        var responseType = _responseMessagesAssembly.GetType(
            _responseMessagesNamespace + Encoding.UTF8.GetString(context.Headers["trq.responseType"])
        )!;
        var messageBytes = (byte[])context.Message.Value;
        var message = JsonSerializer.Deserialize(messageBytes, responseType, _options)!;

        if (context.Headers.Any(kv => kv.Key == "trq.isResponse"))
        {
            var requestId = Encoding.UTF8.GetString(context.Headers["trq.requestId"]);
            await _requestDispatcher.ResponseSink.WriteAsync((requestId, message));
        }
        else
        {
            // @@TODO!!: Handle failures!
            if (message is ArchiveThingAttachmentsProgress thingProgress)
            {
                await _mediator.Publish(
                    new ThingEvents.AttachmentsArchivingProgress.AttachmentsArchivingProgressEvent
                    {
                        SubmitterId = thingProgress.SubmitterId,
                        ThingId = thingProgress.ThingId,
                        Percent = thingProgress.Percent
                    }
                );
            }
            else if (message is ArchiveThingAttachmentsSuccessResult thingSuccessResult)
            {
                await _mediator.Publish(
                    new ThingEvents.AttachmentsArchivingCompleted.AttachmentsArchivingCompletedEvent
                    {
                        SubmitterId = thingSuccessResult.SubmitterId,
                        ThingId = thingSuccessResult.ThingId,
                        Input = thingSuccessResult.Input
                    },
                    addToAdditionalSinks: true
                );
            }
            else if (message is ArchiveSettlementProposalAttachmentsProgress proposalProgress)
            {
                await _mediator.Publish(
                    new SettlementEvents.AttachmentsArchivingProgress.AttachmentsArchivingProgressEvent
                    {
                        SubmitterId = proposalProgress.SubmitterId,
                        ProposalId = proposalProgress.ProposalId,
                        Percent = proposalProgress.Percent
                    }
                );
            }
            else if (message is ArchiveSettlementProposalAttachmentsSuccessResult proposalSuccessResult)
            {
                await _mediator.Publish(
                    new SettlementEvents.AttachmentsArchivingCompleted.AttachmentsArchivingCompletedEvent
                    {
                        SubmitterId = proposalSuccessResult.SubmitterId,
                        ProposalId = proposalSuccessResult.ProposalId,
                        Input = proposalSuccessResult.Input
                    },
                    addToAdditionalSinks: true
                );
            }
        }

        await next(context);
    }
}
