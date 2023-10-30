using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Misc;

namespace Application.User.Queries.GetNonceForSiwe;

public class GetNonceForSiweQuery : IRequest<HandleResult<string>>
{
    public required string Address { get; init; }
}

internal class Validator : AbstractValidator<GetNonceForSiweQuery>
{
    public Validator(IEthereumAddressFormatter addressFormatter)
    {
        RuleFor(q => q.Address).Must(a => addressFormatter.IsValidEIP55EncodedAddress(a));
    }
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
        var nonce = _totpProvider.GenerateTotpFor(query.Address.HexToByteArray());
        return Task.FromResult(new HandleResult<string> { Data = nonce });
    }
}
