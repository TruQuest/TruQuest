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

namespace Application.Settlement.Commands.CloseVerifierLottery;

[ExecuteInTxn]
public class CloseVerifierLotteryCommand : DeferredTaskCommand, IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required byte[] Data { get; init; }
    public required byte[] UserXorData { get; init; }
    public required long EndBlock { get; init; }
    public required long TaskId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId),
            (ActivityTags.SettlementProposalId, SettlementProposalId),
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
    private readonly IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository _claimedLotterySpotEventRepository;
    private readonly IJoinedSettlementProposalAssessmentVerifierLotteryEventRepository _joinedLotteryEventRepository;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IL1BlockchainQueryable l1BlockchainQueryable,
        IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository claimedLotterySpotEventRepository,
        IJoinedSettlementProposalAssessmentVerifierLotteryEventRepository joinedLotteryEventRepository
    )
    {
        _logger = logger;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
        _l1BlockchainQueryable = l1BlockchainQueryable;
        _claimedLotterySpotEventRepository = claimedLotterySpotEventRepository;
        _joinedLotteryEventRepository = joinedLotteryEventRepository;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        // @@TODO!!: Make idempotent.
        var thingId = command.ThingId.ToByteArray();
        var proposalId = command.SettlementProposalId.ToByteArray();

        bool expired = await _contractCaller.CheckSettlementProposalAssessmentVerifierLotteryExpired(thingId, proposalId);
        Debug.Assert(expired);

        var endBlockHash = await _l1BlockchainQueryable.GetBlockHash(command.EndBlock);
        BigInteger maxNonce = await _contractCaller.GetSettlementProposalAssessmentVerifierLotteryMaxNonce();

        var nonce = (long)(
            (
                new BigInteger(command.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(endBlockHash, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        int numVerifiers = await _contractCaller.GetSettlementProposalAssessmentVerifierLotteryNumVerifiers();

        // ordered from oldest to newest
        var spotClaimedEvents = await _claimedLotterySpotEventRepository.FindAllFor(
            command.ThingId, command.SettlementProposalId
        );

        // @@NOTE: User could only have claimed a spot if he was one of the thing verifiers, meaning,
        // he must have joined and won the thing lottery before, meaning, he must be a registered user,
        // since unregistered users are currently excluded.

        var winnerClaimants = spotClaimedEvents
            .Select(
                (e, i) =>
                {
                    e.SetNonce((long)(
                        (
                            new BigInteger(e.UserData.HexToByteArray(), isUnsigned: true, isBigEndian: true) ^
                            new BigInteger(command.UserXorData, isUnsigned: true, isBigEndian: true)
                        ) % maxNonce
                    ));

                    return (
                        UserId: e.UserId!,
                        e.WalletAddress,
                        Index: i,
                        Nonce: e.Nonce!.Value
                    );
                }
            )
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.Index)
            .Take(numVerifiers / 2) // @@TODO: Config.
            .OrderBy(e => e.Index)
            .ToList();

        Debug.Assert(winnerClaimants.Count <= numVerifiers / 2);

        var spotClaimants = await _contractCaller.GetSettlementProposalAssessmentVerifierLotterySpotClaimants(thingId, proposalId);
        foreach (var winnerClaimant in winnerClaimants)
        {
            var claimantAtIndex = spotClaimants.ElementAtOrDefault(new Index(winnerClaimant.Index));
            if (claimantAtIndex != winnerClaimant.WalletAddress)
            {
                throw new Exception("Incorrect claimant index selection");
            }
        }

        await _claimedLotterySpotEventRepository.UpdateNoncesFor(spotClaimedEvents);
        await _claimedLotterySpotEventRepository.SaveChanges();

        int numRequiredParticipants = numVerifiers - winnerClaimants.Count;

        // ordered from oldest to newest
        var joinedEvents = await _joinedLotteryEventRepository.FindAllFor(command.ThingId, command.SettlementProposalId);

        var winnerParticipants = joinedEvents
            .Select((e, i) =>
            {
                if (e.UserId != null)
                {
                    e.SetNonce((long)(
                        (
                            new BigInteger(e.UserData.HexToByteArray(), isUnsigned: true, isBigEndian: true) ^
                            new BigInteger(command.UserXorData, isUnsigned: true, isBigEndian: true)
                        ) % maxNonce
                    ));
                }

                return (
                    e.UserId,
                    e.WalletAddress,
                    Index: i,
                    e.Nonce
                );
            })
            .Where(e => e.Nonce != null)
            .OrderBy(e => Math.Abs(nonce - e.Nonce!.Value))
                .ThenBy(e => e.Index)
            .Take(numRequiredParticipants)
            .OrderBy(e => e.Index)
            .ToList();

        await _joinedLotteryEventRepository.UpdateNoncesFor(joinedEvents);
        await _joinedLotteryEventRepository.SaveChanges();

        if (winnerParticipants.Count == numRequiredParticipants)
        {
            var participants = await _contractCaller.GetSettlementProposalAssessmentVerifierLotteryParticipants(thingId, proposalId);
            foreach (var winnerParticipant in winnerParticipants)
            {
                var participantAtIndex = participants.ElementAtOrDefault(new Index(winnerParticipant.Index));
                if (participantAtIndex != winnerParticipant.WalletAddress)
                {
                    throw new Exception("Incorrect winner index selection");
                }
            }

            await _contractCaller.CloseSettlementProposalAssessmentVerifierLotteryWithSuccess(
                thingId,
                proposalId,
                data: command.Data,
                userXorData: command.UserXorData,
                hashOfL1EndBlock: endBlockHash,
                winnerClaimantIndices: winnerClaimants.Select(w => (ulong)w.Index).ToList(),
                winnerIndices: winnerParticipants.Select(e => (ulong)e.Index).ToList()
            );

            _logger.LogInformation(
                $"Closed settlement proposal {SettlementProposalId} assessment verifier lottery with success.\n" +
                $"Orchestrator nonce: {Nonce}\n" +
                $"Claimants: {JoinedNumClaimants}\n" +
                $"Claimants selected: {WinnerNumClaimants}\n" +
                $"Participants: {JoinedNumParticipants}\n" +
                $"Participants selected: {WinnerNumParticipants}",
                command.SettlementProposalId, nonce, spotClaimedEvents.Count, winnerClaimants.Count,
                joinedEvents.Count, winnerParticipants.Count
            );
        }
        else
        {
            await _contractCaller.CloseSettlementProposalAssessmentVerifierLotteryInFailure(
                thingId, proposalId, winnerClaimants.Count + winnerParticipants.Count
            );

            _logger.LogInformation(
                $"Closed settlement proposal {SettlementProposalId} assessment verifier lottery in failure. Not enough participants.\n" +
                $"Required: {RequiredNumVerifiers}\n" +
                $"Claimants: {JoinedNumClaimants}\n" +
                $"Claimants selected: {WinnerNumClaimants}\n" +
                $"Participants: {JoinedNumParticipants}\n" +
                $"Participants selected: {WinnerNumParticipants}",
                command.SettlementProposalId, numVerifiers, spotClaimedEvents.Count,
                winnerClaimants.Count, joinedEvents.Count, winnerParticipants.Count
            );
        }

        await _taskRepository.SetCompletedStateFor(command.TaskId);

        return VoidResult.Instance;
    }
}
