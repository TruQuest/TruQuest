using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.User.Commands.UnsubThenSubToUpdates;

public class UnsubThenSubToUpdatesCommand : IRequest<VoidResult>
{
    public required string UpdateStreamIdentifierToUnsub { get; init; }
    public required string UpdateStreamIdentifierToSub { get; init; }
}

internal class UnsubThenSubToUpdatesCommandHandler : IRequestHandler<UnsubThenSubToUpdatesCommand, VoidResult>
{
    private readonly IConnectionIdProvider _connectionIdProvider;
    private readonly IClientNotifier _clientNotifier;

    public UnsubThenSubToUpdatesCommandHandler(
        IConnectionIdProvider connectionIdProvider,
        IClientNotifier clientNotifier
    )
    {
        _connectionIdProvider = connectionIdProvider;
        _clientNotifier = clientNotifier;
    }

    public async Task<VoidResult> Handle(UnsubThenSubToUpdatesCommand command, CancellationToken ct)
    {
        await _clientNotifier.UnsubscribeFromStream(
            _connectionIdProvider.ConnectionId, command.UpdateStreamIdentifierToUnsub
        );
        await _clientNotifier.SubscribeToStream(
            _connectionIdProvider.ConnectionId, command.UpdateStreamIdentifierToSub
        );

        return VoidResult.Instance;
    }
}