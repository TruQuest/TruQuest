using GoThataway;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.User.Commands.UnsubThenSubToUpdates;

public class UnsubThenSubToUpdatesCommand : IRequest<VoidResult>
{
    public required string UpdateStreamIdentifierToUnsub { get; init; }
    public required string UpdateStreamIdentifierToSub { get; init; }
}

internal class Validator : AbstractValidator<UnsubThenSubToUpdatesCommand>
{
    public Validator()
    {
        RuleFor(c => c.UpdateStreamIdentifierToUnsub).Must(_beAValidStreamIdentifier);
        RuleFor(c => c.UpdateStreamIdentifierToSub).Must(_beAValidStreamIdentifier);
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

public class UnsubThenSubToUpdatesCommandHandler : IRequestHandler<UnsubThenSubToUpdatesCommand, VoidResult>
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
