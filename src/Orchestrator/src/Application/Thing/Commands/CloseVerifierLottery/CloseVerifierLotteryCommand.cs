using System.Numerics;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;
using Application.Common.Misc;

namespace Application.Thing.Commands.CloseVerifierLottery;

internal class CloseVerifierLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required byte[] Data { get; init; }
    public required byte[] UserXorData { get; init; }
    public required long EndBlock { get; init; }
}

internal class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;
    private readonly IContractCaller _contractCaller;
    private readonly IL1BlockchainQueryable _l1BlockchainQueryable;
    private readonly IJoinedThingSubmissionVerifierLotteryEventRepository _joinedLotteryEventRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        IContractCaller contractCaller,
        IL1BlockchainQueryable l1BlockchainQueryable,
        IJoinedThingSubmissionVerifierLotteryEventRepository joinedLotteryEventRepository,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _logger = logger;
        _contractCaller = contractCaller;
        _l1BlockchainQueryable = l1BlockchainQueryable;
        _joinedLotteryEventRepository = joinedLotteryEventRepository;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        var thingId = command.ThingId.ToByteArray();
        bool expired = await _contractCaller.CheckThingSubmissionVerifierLotteryExpired(thingId);
        Debug.Assert(expired);

        var endBlockHash = await _l1BlockchainQueryable.GetBlockHash(command.EndBlock);
        Debug.Assert(endBlockHash.Length == 32);

        BigInteger maxNonce = await _contractCaller.GetThingSubmissionVerifierLotteryMaxNonce();

        var nonce = (long)(
            (
                new BigInteger(command.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(endBlockHash, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        _logger.LogInformation("Thing {ThingId} Lottery: Orchestrator nonce = {Nonce}", command.ThingId, nonce);

        int numVerifiers = await _contractStorageQueryable.GetThingSubmissionNumVerifiers();

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
            // @@??: Retrieve participants array?
            foreach (var @event in winnerEventsIndexed)
            {
                var user = await _contractStorageQueryable.GetThingSubmissionVerifierLotteryParticipantAt(
                    thingId,
                    (int)@event.Index
                );
                if (user.ToLower() != @event.Event.UserId)
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

        return VoidResult.Instance;
    }
}