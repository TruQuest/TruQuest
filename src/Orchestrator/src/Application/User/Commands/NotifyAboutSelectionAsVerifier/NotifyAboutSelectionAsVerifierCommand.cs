using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;

namespace Application.User.Commands.NotifyAboutSelectionAsVerifier;

public class NotifyAboutSelectionAsVerifierCommand : IRequest<VoidResult>
{
    public Guid ThingId { get; init; }
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
        _logger.LogInformation("User {UserId} selected as verifier for thing {ThingId}", command.UserId, command.ThingId);
        return VoidResult.Instance;
    }
}