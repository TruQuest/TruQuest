using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;

namespace Application.User.Commands.NotifyAboutSelectionAsVerifier;

public class NotifyAboutSelectionAsVerifierCommand : IRequest<VoidResult>
{
    public Guid? ThingId { get; init; }
    public Guid? SettlementProposalId { get; init; }
    public required string UserId { get; init; }
}

internal class NotifyAboutSelectionAsVerifierCommandHandler : IRequestHandler<NotifyAboutSelectionAsVerifierCommand, VoidResult>
{
    private readonly ILogger<NotifyAboutSelectionAsVerifierCommandHandler> _logger;

    public NotifyAboutSelectionAsVerifierCommandHandler(
        ILogger<NotifyAboutSelectionAsVerifierCommandHandler> logger
    )
    {
        _logger = logger;
    }

    public async Task<VoidResult> Handle(NotifyAboutSelectionAsVerifierCommand command, CancellationToken ct)
    {
        if (command.ThingId != null)
        {
            _logger.LogInformation("User {UserId} selected as submission verifier for thing {ThingId}", command.UserId, command.ThingId);
        }
        else if (command.SettlementProposalId != null)
        {
            _logger.LogInformation("User {UserId} selected as assessment verifier for settlement proposal {SettlementProposalId}", command.UserId, command.SettlementProposalId);
        }

        return VoidResult.Instance;
    }
}