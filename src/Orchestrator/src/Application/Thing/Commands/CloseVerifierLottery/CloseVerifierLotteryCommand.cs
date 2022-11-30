using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;

namespace Application.Thing.Commands.CloseVerifierLottery;

public class CloseVerifierLotteryCommand : IRequest<VoidResult>
{
    public Guid ThingId { get; init; }
    public required byte[] Data { get; init; }
}

internal class CloseVerifierLotteryCommandHandler : IRequestHandler<CloseVerifierLotteryCommand, VoidResult>
{
    private readonly ILogger<CloseVerifierLotteryCommandHandler> _logger;

    public CloseVerifierLotteryCommandHandler(ILogger<CloseVerifierLotteryCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<VoidResult> Handle(CloseVerifierLotteryCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Closing verifier lottery for thing {ThingId}", command.ThingId);
        return VoidResult.Instance;
    }
}