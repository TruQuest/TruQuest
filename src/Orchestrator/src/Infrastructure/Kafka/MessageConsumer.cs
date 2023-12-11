using System.Text;

using Microsoft.Extensions.Logging;

using KafkaFlow;
using GoThataway;

using Application.Common.Interfaces;
using Application.Common.Messages.Responses;
using ThingEvents = Application.Thing.Events;
using SettlementEvents = Application.Settlement.Events;

namespace Infrastructure.Kafka;

internal class MessageConsumer : IMessageMiddleware
{
    private readonly ILogger<MessageConsumer> _logger;
    private readonly IRequestDispatcher _requestDispatcher;
    private readonly Thataway _thataway;

    public MessageConsumer(
        ILogger<MessageConsumer> logger,
        IRequestDispatcher requestDispatcher,
        Thataway thataway
    )
    {
        _logger = logger;
        _requestDispatcher = requestDispatcher;
        _thataway = thataway;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var message = context.Message.Value;

        if (context.Headers.Any(kv => kv.Key == "trq.isResponse"))
        {
            var requestId = Encoding.UTF8.GetString(context.Headers["trq.requestId"]);
            await _requestDispatcher.ResponseSink.WriteAsync((requestId, message));
        }
        else
        {
            if (message is ArchiveThingAttachmentsProgress thingProgress)
            {
                await _thataway.Dispatch(
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
                await _thataway.Dispatch(
                    new ThingEvents.AttachmentsArchivingCompleted.AttachmentsArchivingCompletedEvent
                    {
                        SubmitterId = thingSuccessResult.SubmitterId,
                        ThingId = thingSuccessResult.ThingId,
                        Input = thingSuccessResult.Input
                    }
                );
            }
            else if (message is ArchiveThingAttachmentsFailureResult thingFailureResult)
            {
                await _thataway.Dispatch(
                    new ThingEvents.AttachmentsArchivingFailed.AttachmentsArchivingFailedEvent
                    {
                        SubmitterId = thingFailureResult.SubmitterId,
                        ThingId = thingFailureResult.ThingId,
                        ErrorMessage = thingFailureResult.ErrorMessage
                    }
                );
            }
            else if (message is ArchiveSettlementProposalAttachmentsProgress proposalProgress)
            {
                await _thataway.Dispatch(
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
                await _thataway.Dispatch(
                    new SettlementEvents.AttachmentsArchivingCompleted.AttachmentsArchivingCompletedEvent
                    {
                        SubmitterId = proposalSuccessResult.SubmitterId,
                        ProposalId = proposalSuccessResult.ProposalId,
                        Input = proposalSuccessResult.Input
                    }
                );
            }
            else if (message is ArchiveSettlementProposalAttachmentsFailureResult proposalFailureResult)
            {
                await _thataway.Dispatch(
                    new SettlementEvents.AttachmentsArchivingFailed.AttachmentsArchivingFailedEvent
                    {
                        SubmitterId = proposalFailureResult.SubmitterId,
                        ProposalId = proposalFailureResult.ProposalId,
                        ErrorMessage = proposalFailureResult.ErrorMessage
                    }
                );
            }
        }

        await next(context);
    }
}
