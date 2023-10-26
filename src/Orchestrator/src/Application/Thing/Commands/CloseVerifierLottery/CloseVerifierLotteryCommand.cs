using System.Numerics;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Common.Misc;
using Application.Common.Attributes;
using Application.Common.Models.IM;

namespace Application.Thing.Commands.CloseVerifierLottery;

[ExecuteInTxn]
internal class CloseVerifierLotteryCommand : DeferredTaskCommand, IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required byte[] Data { get; init; }
    public required byte[] UserXorData { get; init; }
    public required long EndBlock { get; init; }
    public required long TaskId { get; init; }
}

internal class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;
    private readonly IL1BlockchainQueryable _l1BlockchainQueryable;
    private readonly IJoinedThingSubmissionVerifierLotteryEventRepository _joinedLotteryEventRepository;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IL1BlockchainQueryable l1BlockchainQueryable,
        IJoinedThingSubmissionVerifierLotteryEventRepository joinedLotteryEventRepository
    )
    {
        _logger = logger;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
        _l1BlockchainQueryable = l1BlockchainQueryable;
        _joinedLotteryEventRepository = joinedLotteryEventRepository;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        var thingId = command.ThingId.ToByteArray();
        bool expired = await _contractCaller.CheckThingSubmissionVerifierLotteryExpired(thingId);
        Debug.Assert(expired);

        var endBlockHash = await _l1BlockchainQueryable.GetBlockHash(command.EndBlock);
        BigInteger maxNonce = await _contractCaller.GetThingSubmissionVerifierLotteryMaxNonce();

        var nonce = (long)(
            (
                new BigInteger(command.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(endBlockHash, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        _logger.LogInformation("Thing {ThingId} Lottery: Orchestrator nonce = {Nonce}", command.ThingId, nonce);

        int numVerifiers = await _contractCaller.GetThingSubmissionLotteryNumVerifiers();

        var joinedEvents = await _joinedLotteryEventRepository.FindAllFor(command.ThingId);
        foreach (var @event in joinedEvents)
        {
            @event.SetNonce((long)(
                (
                    new BigInteger(@event.UserData!.HexToByteArray(), isUnsigned: true, isBigEndian: true) ^
                    new BigInteger(command.UserXorData, isUnsigned: true, isBigEndian: true)
                ) % maxNonce
            ));
        }

        var winnerEventsIndexed = joinedEvents
            .Select((e, i) => (Index: (ulong)i, Event: e))
            .OrderBy(e => Math.Abs(nonce - e.Event.Nonce!.Value))
                .ThenBy(e => e.Index)
            .Take(numVerifiers)
            .OrderBy(e => e.Index)
            .ToList();

        await _joinedLotteryEventRepository.UpdateNoncesFor(joinedEvents);
        await _joinedLotteryEventRepository.SaveChanges();

        if (winnerEventsIndexed.Count == numVerifiers)
        {
            var participants = await _contractCaller.GetThingSubmissionVerifierLotteryParticipants(thingId);
            foreach (var @event in winnerEventsIndexed)
            {
                var participantAtIndex = participants.ElementAtOrDefault(new Index((int)@event.Index));
                if (participantAtIndex?.Substring(2).ToLower() != @event.Event.UserId)
                {
                    throw new Exception("Incorrect winner selection");
                }
            }

            await _contractCaller.CloseThingSubmissionVerifierLotteryWithSuccess(
                thingId,
                data: command.Data,
                userXorData: command.UserXorData,
                hashOfL1EndBlock: endBlockHash,
                winnerIndices: winnerEventsIndexed.Select(e => e.Index).ToList()
            );
        }
        else
        {
            _logger.LogInformation(
                "Thing {ThingId} Verifier Selection Lottery: Not enough participants.\n" +
                "Required: {RequiredNumVerifiers}.\n" +
                "Joined: {JoinedNumVerifiers}.",
                command.ThingId, numVerifiers, joinedEvents.Count
            );

            await _contractCaller.CloseThingSubmissionVerifierLotteryInFailure(thingId, joinedEvents.Count);
        }

        await _taskRepository.SetCompletedStateFor(command.TaskId);

        return VoidResult.Instance;
    }
}
