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

namespace Application.Settlement.Commands.CloseVerifierLottery;

[ExecuteInTxn]
internal class CloseVerifierLotteryCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
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
    private readonly IThingAssessmentVerifierLotterySpotClaimedEventRepository _lotterySpotClaimedEventRepository;
    private readonly IJoinedThingAssessmentVerifierLotteryEventRepository _joinedLotteryEventRepository;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        ITaskRepository taskRepository,
        IContractCaller contractCaller,
        IL1BlockchainQueryable l1BlockchainQueryable,
        IThingAssessmentVerifierLotterySpotClaimedEventRepository lotterySpotClaimedEventRepository,
        IJoinedThingAssessmentVerifierLotteryEventRepository joinedLotteryEventRepository
    )
    {
        _logger = logger;
        _taskRepository = taskRepository;
        _contractCaller = contractCaller;
        _l1BlockchainQueryable = l1BlockchainQueryable;
        _lotterySpotClaimedEventRepository = lotterySpotClaimedEventRepository;
        _joinedLotteryEventRepository = joinedLotteryEventRepository;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        // @@TODO!!: Make idempotent.
        var thingId = command.ThingId.ToByteArray();
        var proposalId = command.SettlementProposalId.ToByteArray();

        bool expired = await _contractCaller.CheckThingAssessmentVerifierLotteryExpired(thingId, proposalId);
        Debug.Assert(expired);

        var endBlockHash = await _l1BlockchainQueryable.GetBlockHash(command.EndBlock);
        BigInteger maxNonce = await _contractCaller.GetThingAssessmentVerifierLotteryMaxNonce();

        var nonce = (long)(
            (
                new BigInteger(command.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(endBlockHash, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        int numVerifiers = await _contractCaller.GetThingAssessmentLotteryNumVerifiers();

        // ordered from oldest to newest
        var spotClaimedEvents = await _lotterySpotClaimedEventRepository.FindAllFor(
            command.ThingId, command.SettlementProposalId
        );

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
                        e.UserId,
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

        var spotClaimants = await _contractCaller.GetThingAssessmentVerifierLotterySpotClaimants(thingId, proposalId);
        foreach (var winnerClaimant in winnerClaimants)
        {
            var claimantAtIndex = spotClaimants.ElementAtOrDefault(new Index(winnerClaimant.Index));
            if (claimantAtIndex?.Substring(2).ToLower() != winnerClaimant.UserId)
            {
                throw new Exception("Incorrect claimant index selection");
            }
        }

        await _lotterySpotClaimedEventRepository.UpdateUserDataAndNoncesFor(spotClaimedEvents);
        await _lotterySpotClaimedEventRepository.SaveChanges();

        _logger.LogInformation(
            "Proposal {ProposalId} verifier lottery: {NumClaimants} spots claimed",
            command.SettlementProposalId,
            winnerClaimants.Count
        );

        int numRequiredParticipants = numVerifiers - winnerClaimants.Count;

        _logger.LogInformation(
            "Proposal {ProposalId} verifier lottery: {NumRequiredParticipants} participants required",
            command.SettlementProposalId,
            numRequiredParticipants
        );

        // ordered from oldest to newest
        var joinedEvents = await _joinedLotteryEventRepository.FindAllFor(command.ThingId, command.SettlementProposalId);

        var winnerParticipants = joinedEvents
            .Select((e, i) =>
            {
                e.SetNonce((long)(
                    (
                        new BigInteger(e.UserData!.HexToByteArray(), isUnsigned: true, isBigEndian: true) ^
                        new BigInteger(command.UserXorData, isUnsigned: true, isBigEndian: true)
                    ) % maxNonce
                ));

                return (
                    e.UserId,
                    Index: i,
                    Nonce: e.Nonce!.Value
                );
            })
            .OrderBy(e => Math.Abs(nonce - e.Nonce))
                .ThenBy(e => e.Index)
            .Take(numRequiredParticipants)
            .OrderBy(e => e.Index)
            .ToList();

        await _joinedLotteryEventRepository.UpdateNoncesFor(joinedEvents);
        await _joinedLotteryEventRepository.SaveChanges();

        if (winnerParticipants.Count == numRequiredParticipants)
        {
            var participants = await _contractCaller.GetThingAssessmentVerifierLotteryParticipants(thingId, proposalId);
            foreach (var winnerParticipant in winnerParticipants)
            {
                var participantAtIndex = participants.ElementAtOrDefault(new Index(winnerParticipant.Index));
                if (participantAtIndex?.Substring(2).ToLower() != winnerParticipant.UserId)
                {
                    throw new Exception("Incorrect winner index selection");
                }
            }

            await _contractCaller.CloseThingAssessmentVerifierLotteryWithSuccess(
                thingId,
                proposalId,
                data: command.Data,
                userXorData: command.UserXorData,
                hashOfL1EndBlock: endBlockHash,
                winnerClaimantIndices: winnerClaimants.Select(w => (ulong)w.Index).ToList(),
                winnerIndices: winnerParticipants.Select(e => (ulong)e.Index).ToList()
            );
        }
        else
        {
            _logger.LogInformation(
                "Proposal {ProposalId} Verifier Selection Lottery: Not enough participants.\n" +
                "Required: {RequiredNumVerifiers}.\n" +
                "Joined: {JoinedNumVerifiers}.",
                command.SettlementProposalId, numVerifiers, winnerClaimants.Count + winnerParticipants.Count
            );

            await _contractCaller.CloseThingAssessmentVerifierLotteryInFailure(
                thingId, proposalId, winnerClaimants.Count + winnerParticipants.Count
            );
        }

        await _taskRepository.SetCompletedStateFor(command.TaskId);

        return VoidResult.Instance;
    }
}
