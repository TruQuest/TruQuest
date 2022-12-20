using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;

namespace Application.Thing.Commands.CloseVerifierLottery;

internal class CloseVerifierLotteryCommand : IRequest<VoidResult>
{
    public long LatestIncludedBlockNumber { get; init; }
    public Guid ThingId { get; init; }
    public required byte[] Data { get; init; }
}

internal class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;
    private readonly IContractCaller _contractCaller;
    private readonly IPreJoinedVerifierLotteryEventRepository _preJoinedVerifierLotteryEventRepository;
    private readonly IJoinedVerifierLotteryEventRepository _joinedVerifierLotteryEventRepository;
    private readonly IContractStorageQueryable _contractStorageQueryable;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        IContractCaller contractCaller,
        IPreJoinedVerifierLotteryEventRepository preJoinedVerifierLotteryEventRepository,
        IJoinedVerifierLotteryEventRepository joinedVerifierLotteryEventRepository,
        IContractStorageQueryable contractStorageQueryable
    )
    {
        _logger = logger;
        _contractCaller = contractCaller;
        _preJoinedVerifierLotteryEventRepository = preJoinedVerifierLotteryEventRepository;
        _joinedVerifierLotteryEventRepository = joinedVerifierLotteryEventRepository;
        _contractStorageQueryable = contractStorageQueryable;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        var thingId = command.ThingId.ToByteArray();
        var nonce = (decimal)await _contractCaller.ComputeNonce(thingId, command.Data);
        int numVerifiers = await _contractStorageQueryable.GetNumVerifiers();

        var winnerEvents = await _joinedVerifierLotteryEventRepository.FindWithClosestNonces(
            thingId: command.ThingId,
            latestBlockNumber: command.LatestIncludedBlockNumber,
            nonce: nonce,
            count: numVerifiers
        );

        if (winnerEvents.Count == numVerifiers)
        {
            var lotteryWinners = await _preJoinedVerifierLotteryEventRepository.GetLotteryWinnerIndices(
                command.ThingId,
                winnerEvents.Select(e => e.UserId)
            );

            foreach (var winner in lotteryWinners)
            {
                var user = await _contractStorageQueryable.GetVerifierLotteryParticipantAt(
                    thingId,
                    (int)winner.Index
                );
                if (user.ToLower() != winner.UserId)
                {
                    throw new Exception("Incorrect winner selection");
                }
            }

            await _contractCaller.CloseVerifierLotteryWithSuccess(
                thingId,
                command.Data,
                lotteryWinners.Select(w => w.Index).ToList()
            );
        }

        return VoidResult.Instance;
    }
}