using System.Reflection;

using GoThataway;

using Domain.Results;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Common.Middlewares.Request;

public class RestrictedAccessMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IWhitelistQueryable _whitelistQueryable;

    public RestrictedAccessMiddleware(
        ICurrentPrincipal currentPrincipal,
        IWhitelistQueryable whitelistQueryable
    )
    {
        _currentPrincipal = currentPrincipal;
        _whitelistQueryable = whitelistQueryable;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
    {
        var authorizeAttributes = request
            .GetType()
            .GetCustomAttributes<RequireAuthorizationAttribute>();

        if (
            authorizeAttributes.Any() &&
            (
                _currentPrincipal.Id == null ||
                !await _whitelistQueryable.CheckIsWhitelisted(_currentPrincipal.Id)
            )
        )
        {
            return new()
            {
                Error = new HandleError("Sorry, the access is currently restricted. Email me at admin@truquest.io to get access")
            };
        }

        return await next();
    }
}
