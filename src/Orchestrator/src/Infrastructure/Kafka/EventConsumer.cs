using System.Text;

using Microsoft.Extensions.DependencyInjection;

using KafkaFlow;

using Domain.Aggregates;
using Domain.Results;
using Application;
using Application.Common.Errors;
using Application.Settlement.Commands.FinalizeAssessmentPoll;
using Application.Settlement.Commands.PrepareForAssessmentPoll;
using Application.Thing.Commands.ArchiveDueToFailedLottery;
using Application.Thing.Commands.FinalizeValidationPoll;
using Application.Thing.Commands.PrepareForValidationPoll;
using Application.User.Commands.NotifyWatchers;
using Application.User.Common.Models.IM;

using Infrastructure.Kafka.Events;

namespace Infrastructure.Kafka;

internal class EventConsumer : IMessageMiddleware
{
    private const string _messageTypeHeaderName = "Type";
    private const string _blockNumberHeaderName = "BlockNumber";
    private const string _txnIndexHeaderName = "TxnIndex";
    private const string _txnHashHeaderName = "TxnHash";

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<SenderWrapper>();

        // @@NOTE: We do this instead of using KafkaFlow's typed handler mechanism, because
        // on every retry we need a completely new scope from which SenderWrapper, ISender, all
        // repos, etc. will get resolved, but when using typed handlers with Scoped lifetime
        // the handlers and their dependencies only get resolved once.

        VoidResult result;
        var message = context.Message.Value;
        if (message is ThingFundedEvent)
        {
            result = await sender.Send(
                new Application.Thing.Commands.InitVerifierLottery.InitVerifierLotteryCommand
                {
                    ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))
                },
                addToAdditionalSinks: true
            );
        }
        else if (message is ThingValidationVerifierLotteryClosedInFailureEvent)
        {
            result = await sender.Send(
                new ArchiveDueToFailedLotteryCommand
                {
                    ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))
                },
                addToAdditionalSinks: true
            );
        }
        else if (message is ThingValidationVerifierLotteryClosedWithSuccessEvent validationLotteryClosedWithSuccessEvent)
        {
            result = await sender.Send(
                new PrepareForValidationPollCommand
                {
                    PollInitBlockNumber = long.Parse(
                        Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])
                    ),
                    PollInitTxnIndex = int.Parse(
                        Encoding.UTF8.GetString((byte[])context.Headers[_txnIndexHeaderName])
                    ),
                    PollInitTxnHash = Encoding.UTF8.GetString((byte[])context.Headers[_txnHashHeaderName]),
                    ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                    Orchestrator = validationLotteryClosedWithSuccessEvent.Orchestrator,
                    Data = validationLotteryClosedWithSuccessEvent.Data,
                    UserXorData = validationLotteryClosedWithSuccessEvent.UserXorData,
                    HashOfL1EndBlock = validationLotteryClosedWithSuccessEvent.HashOfL1EndBlock,
                    Nonce = validationLotteryClosedWithSuccessEvent.Nonce,
                    WinnerWalletAddresses = validationLotteryClosedWithSuccessEvent.WinnerWalletAddresses
                },
                addToAdditionalSinks: true
            );
        }
        else if (message is ThingValidationPollFinalizedEvent validationPollFinalizedEvent)
        {
            result = await sender.Send(
                new FinalizeValidationPollCommand
                {
                    ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                    Decision = (ValidationDecision)validationPollFinalizedEvent.Decision,
                    VoteAggIpfsCid = validationPollFinalizedEvent.VoteAggIpfsCid,
                    RewardedVerifiers = validationPollFinalizedEvent.RewardedVerifiers,
                    SlashedVerifiers = validationPollFinalizedEvent.SlashedVerifiers
                },
                addToAdditionalSinks: true
            );
        }
        else if (message is SettlementProposalFundedEvent proposalFundedEvent)
        {
            result = await sender.Send(
                new Application.Settlement.Commands.InitVerifierLottery.InitVerifierLotteryCommand
                {
                    ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                    SettlementProposalId = proposalFundedEvent.SettlementProposalId
                },
                addToAdditionalSinks: true
            );
        }
        else if (message is SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent assessmentLotteryClosedWithSuccessEvent)
        {
            result = await sender.Send(
                new PrepareForAssessmentPollCommand
                {
                    InitBlockNumber = long.Parse(
                        Encoding.UTF8.GetString((byte[])context.Headers[_blockNumberHeaderName])
                    ),
                    InitTxnIndex = int.Parse(
                        Encoding.UTF8.GetString((byte[])context.Headers[_txnIndexHeaderName])
                    ),
                    InitTxnHash = Encoding.UTF8.GetString((byte[])context.Headers[_txnHashHeaderName]),
                    ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                    SettlementProposalId = assessmentLotteryClosedWithSuccessEvent.SettlementProposalId,
                    Orchestrator = assessmentLotteryClosedWithSuccessEvent.Orchestrator,
                    Data = assessmentLotteryClosedWithSuccessEvent.Data,
                    UserXorData = assessmentLotteryClosedWithSuccessEvent.UserXorData,
                    HashOfL1EndBlock = assessmentLotteryClosedWithSuccessEvent.HashOfL1EndBlock,
                    Nonce = assessmentLotteryClosedWithSuccessEvent.Nonce,
                    ClaimantWalletAddresses = assessmentLotteryClosedWithSuccessEvent.ClaimantWalletAddresses,
                    WinnerWalletAddresses = assessmentLotteryClosedWithSuccessEvent.WinnerWalletAddresses
                },
                addToAdditionalSinks: true
            );
        }
        else if (message is SettlementProposalAssessmentPollFinalizedEvent assessmentPollFinalizedEvent)
        {
            result = await sender.Send(
                new FinalizeAssessmentPollCommand
                {
                    ThingId = Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)),
                    SettlementProposalId = assessmentPollFinalizedEvent.SettlementProposalId,
                    Decision = (AssessmentDecision)assessmentPollFinalizedEvent.Decision,
                    VoteAggIpfsCid = assessmentPollFinalizedEvent.VoteAggIpfsCid,
                    RewardedVerifiers = assessmentPollFinalizedEvent.RewardedVerifiers,
                    SlashedVerifiers = assessmentPollFinalizedEvent.SlashedVerifiers
                },
                addToAdditionalSinks: true
            );
        }
        else if (message is ThingUpdateEvent thingUpdateEvent)
        {
            result = await sender.Send(
                new NotifyWatchersCommand
                {
                    ItemType = WatchedItemTypeIm.Thing,
                    ItemId = thingUpdateEvent.ThingId,
                    ItemUpdateCategory = (int)thingUpdateEvent.Category,
                    UpdateTimestamp = thingUpdateEvent.UpdateTimestamp,
                    Title = thingUpdateEvent.Title,
                    Details = thingUpdateEvent.Details
                },
                addToAdditionalSinks: true
            );
        }
        else if (message is SettlementProposalUpdateEvent proposalUpdateEvent)
        {
            result = await sender.Send(
                new NotifyWatchersCommand
                {
                    ItemType = WatchedItemTypeIm.SettlementProposal,
                    ItemId = proposalUpdateEvent.SettlementProposalId,
                    ItemUpdateCategory = (int)proposalUpdateEvent.Category,
                    UpdateTimestamp = proposalUpdateEvent.UpdateTimestamp,
                    Title = proposalUpdateEvent.Title,
                    Details = proposalUpdateEvent.Details
                },
                addToAdditionalSinks: true
            );
        }
        else
        {
            throw new InvalidOperationException();
        }

        if (result.Error != null)
        {
            context.Headers["HandleError"] = ((ServerError)result.Error).IsRetryable ? new byte[1] { 1 } : new byte[1] { 0 };
        }

        await next(context);
    }
}
