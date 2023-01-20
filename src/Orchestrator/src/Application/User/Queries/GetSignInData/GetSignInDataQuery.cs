using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.User.Queries.GetSignInData;

public class GetSignInDataQuery : IRequest<HandleResult<GetSignInDataResultVm>> { }

internal class GetSignInDataQueryHandler : IRequestHandler<GetSignInDataQuery, HandleResult<GetSignInDataResultVm>>
{
    private readonly ISigner _signer;

    public GetSignInDataQueryHandler(ISigner signer)
    {
        _signer = signer;
    }

    public Task<HandleResult<GetSignInDataResultVm>> Handle(GetSignInDataQuery query, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        return Task.FromResult(new HandleResult<GetSignInDataResultVm>
        {
            Data = new()
            {
                Timestamp = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Signature = _signer.SignTimestamp(now)
            }
        });
    }
}