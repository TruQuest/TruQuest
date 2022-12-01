using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates.Events;

using Application.Common.Interfaces;

namespace Application.Thing.Commands.CloseVerifierLottery;

public class CloseVerifierLotteryCommand : IRequest<VoidResult>
{
    public long LatestIncludedBlockNumber { get; init; }
    public Guid ThingId { get; init; }
    public required byte[] Data { get; init; }
}

internal class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;
    private readonly IContractCaller _contractCaller;
    private readonly IPreJoinedLotteryEventRepository _preJoinedLotteryEventRepository;
    private readonly IJoinedLotteryEventRepository _joinedLotteryEventRepository;

    public CloseVerifierLotteryCommandHandler(
        ILogger<CloseVerifierLotteryCommandHandler> logger,
        IContractCaller contractCaller,
        IPreJoinedLotteryEventRepository preJoinedLotteryEventRepository,
        IJoinedLotteryEventRepository joinedLotteryEventRepository
    )
    {
        _logger = logger;
        _contractCaller = contractCaller;
        _preJoinedLotteryEventRepository = preJoinedLotteryEventRepository;
        _joinedLotteryEventRepository = joinedLotteryEventRepository;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        var nonce = (decimal)await _contractCaller.ComputeNonce(command.ThingId.ToString(), command.Data);

        var winnerEvents = await _joinedLotteryEventRepository.FindWithClosestNonces(
            thingId: command.ThingId,
            latestBlockNumber: command.LatestIncludedBlockNumber,
            nonce: nonce,
            count: 3
        );

        if (winnerEvents.Count == 3)
        {
            var lotteryWinners = await _preJoinedLotteryEventRepository.GetLotteryWinnerIndices(
                command.ThingId,
                winnerEvents.Select(e => e.UserId)
            );

            await _contractCaller.CloseVerifierLotteryWithSuccess(
                command.ThingId.ToString(),
                command.Data,
                lotteryWinners.Select(w => w.Index).ToList()
            );
        }

        return VoidResult.Instance;
    }
}