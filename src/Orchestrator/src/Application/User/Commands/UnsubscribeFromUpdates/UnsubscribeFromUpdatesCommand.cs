using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.User.Commands.UnsubscribeFromUpdates;

public class UnsubscribeFromUpdatesCommand : IRequest<VoidResult>
{
    public required string UpdateStreamIdentifier { get; init; }
}

internal class Validator : AbstractValidator<UnsubscribeFromUpdatesCommand>
{
    public Validator()
    {
        RuleFor(c => c.UpdateStreamIdentifier).Must(_beAValidStreamIdentifier);
    }

    private bool _beAValidStreamIdentifier(string streamIdentifier)
    {
        var streamIdentifierSplit = streamIdentifier.Split('/');
        return
            streamIdentifierSplit.Length == 3 &&
            streamIdentifierSplit[0] == string.Empty &&
            streamIdentifierSplit[1] is "subjects" or "things" or "proposals" &&
            Guid.TryParse(streamIdentifierSplit[2], out _);
    }
}

internal class UnsubscribeFromUpdatesCommandHandler : IRequestHandler<UnsubscribeFromUpdatesCommand, VoidResult>
{
    private readonly IConnectionIdProvider _connectionIdProvider;
    private readonly IClientNotifier _clientNotifier;

    public UnsubscribeFromUpdatesCommandHandler(
        IConnectionIdProvider connectionIdProvider,
        IClientNotifier clientNotifier
    )
    {
        _connectionIdProvider = connectionIdProvider;
        _clientNotifier = clientNotifier;
    }

    public async Task<VoidResult> Handle(UnsubscribeFromUpdatesCommand command, CancellationToken ct)
    {
        await _clientNotifier.UnsubscribeFromStream(_connectionIdProvider.ConnectionId, command.UpdateStreamIdentifier);
        return VoidResult.Instance;
    }
}