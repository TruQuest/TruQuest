using System.Numerics;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Results;
using Domain.Aggregates;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Common.Misc;
using Application.Common.Attributes;
using Application.Common.Models.IM;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Thing.Commands.CloseVerifierLottery;

[ExecuteInTxn]
public class CloseVerifierLotteryCommand : DeferredTaskCommand, IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required byte[] Data { get; init; }
    public required byte[] UserXorData { get; init; }
    public required long EndBlock { get; init; }
    public required long TaskId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId),
            (ActivityTags.EndBlockNum, EndBlock),
            (ActivityTags.TaskId, TaskId)
        };
    }
}

public class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IContractCaller _contractCaller;
    private readonly IL1BlockchainQueryable _l1BlockchainQueryable;
    private readonly IJoinedThingValidationVerifierLotteryEventRepository _joinedLotteryEventRepository;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IL1BlockchainQueryable l1BlockchainQueryable,
        IJoinedThingValidationVerifierLotteryEventRepository joinedLotteryEventRepository
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
        bool expired = await _contractCaller.CheckThingValidationVerifierLotteryExpired(thingId);
        Debug.Assert(expired);

        var endBlockHash = await _l1BlockchainQueryable.GetBlockHash(command.EndBlock);
        BigInteger maxNonce = await _contractCaller.GetThingValidationVerifierLotteryMaxNonce();

        var nonce = (long)(
            (
                new BigInteger(command.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(endBlockHash, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        int numVerifiers = await _contractCaller.GetThingValidationVerifierLotteryNumVerifiers();

        var joinedEvents = await _joinedLotteryEventRepository.FindAllFor(command.ThingId); // all, even those with UserId == null
        foreach (var @event in joinedEvents.Where(e => e.UserId != null))
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
            .Where(e => e.Event.Nonce != null)
            .OrderBy(e => Math.Abs(nonce - e.Event.Nonce!.Value))
                .ThenBy(e => e.Index)
            .Take(numVerifiers)
            .OrderBy(e => e.Index)
            .ToList();

        // @@TODO: There could be a situation when user joins a lottery without actually registering
        // on the platform and gets a nonce that should win him a verifier spot, but he gets excluded.
        // How to prove to an observer that his exclusion was justified?

        // @@??: Should we even exclude unregistered users ?

        await _joinedLotteryEventRepository.UpdateNoncesFor(joinedEvents);
        await _joinedLotteryEventRepository.SaveChanges();

        if (winnerEventsIndexed.Count == numVerifiers)
        {
            var participants = await _contractCaller.GetThingValidationVerifierLotteryParticipants(thingId);
            foreach (var @event in winnerEventsIndexed)
            {
                var participantAtIndex = participants.ElementAtOrDefault(new Index((int)@event.Index));
                if (participantAtIndex != @event.Event.WalletAddress)
                {
                    throw new Exception("Incorrect winner selection");
                }
            }

            await _contractCaller.CloseThingValidationVerifierLotteryWithSuccess(
                thingId,
                data: command.Data,
                userXorData: command.UserXorData,
                hashOfL1EndBlock: endBlockHash,
                winnerIndices: winnerEventsIndexed.Select(e => e.Index).ToList()
            );

            _logger.LogInformation(
                $"Closed thing {ThingId} validation verifier lottery with success.\n" +
                $"Orchestrator nonce: {Nonce}\n" +
                $"Participants: {JoinedNumParticipants}",
                command.ThingId, nonce, joinedEvents.Count
            );
        }
        else
        {
            await _contractCaller.CloseThingValidationVerifierLotteryInFailure(thingId, joinedEvents.Count);

            _logger.LogInformation(
                $"Closed thing {ThingId} validation verifier lottery in failure. Not enough participants.\n" +
                $"Required: {RequiredNumVerifiers}\n" +
                $"Joined: {JoinedNumParticipants}",
                command.ThingId, numVerifiers, joinedEvents.Count
            );
        }

        await _taskRepository.SetCompletedStateFor(command.TaskId);

        return VoidResult.Instance;
    }
}
