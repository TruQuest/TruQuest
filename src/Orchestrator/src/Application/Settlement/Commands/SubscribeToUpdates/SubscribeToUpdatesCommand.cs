using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Commands.SubscribeToUpdates;

public class SubscribeToUpdatesCommand : IRequest<VoidResult>
{
    public required Guid ProposalId { get; init; }
}

internal class SubscribeToUpdatesCommandHandler : IRequestHandler<SubscribeToUpdatesCommand, VoidResult>
{
    private readonly IConnectionIdProvider _connectionIdProvider;
    private readonly IClientNotifier _clientNotifier;

    public SubscribeToUpdatesCommandHandler(
        IConnectionIdProvider connectionIdProvider,
        IClientNotifier clientNotifier
    )
    {
        _connectionIdProvider = connectionIdProvider;
        _clientNotifier = clientNotifier;
    }

    public async Task<VoidResult> Handle(SubscribeToUpdatesCommand command, CancellationToken ct)
    {
        await _clientNotifier.SubscribeToSettlementProposal(_connectionIdProvider.ConnectionId, command.ProposalId);
        return VoidResult.Instance;
    }
}