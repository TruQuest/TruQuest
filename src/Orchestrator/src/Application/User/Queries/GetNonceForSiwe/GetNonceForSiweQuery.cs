using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.User.Queries.GetNonceForSiwe;

public class GetNonceForSiweQuery : IRequest<HandleResult<string>>
{
    public required string Address { get; init; }
}

internal class GetNonceForSiweQueryHandler : IRequestHandler<GetNonceForSiweQuery, HandleResult<string>>
{
    private readonly ITotpProvider _totpProvider;

    public GetNonceForSiweQueryHandler(ITotpProvider totpProvider)
    {
        _totpProvider = totpProvider;
    }

    public Task<HandleResult<string>> Handle(GetNonceForSiweQuery query, CancellationToken ct)
    {
        var nonce = _totpProvider.GenerateTotpFor(query.Address);
        return Task.FromResult(new HandleResult<string> { Data = nonce });
    }
}
