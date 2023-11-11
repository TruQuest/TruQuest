using System.Reflection;

using MediatR;

using Domain.Results;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Common.Behaviors;

public class RestrictedAccessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly Lazy<IWhitelistQueryable> _whitelistQueryable;
    // @@NOTE: This Lazy stuff is a crutch necessary because of the way f*cking MediatR resolves deps.
    // MediatR resolves pipeline behaviors all at once, before any of them are executed. But the handler
    // itself is resolved "just in time", meaning, when the immediately preceding behavior calls next().
    // This "all at once" resolving of behaviors makes any repos and queryables used in a behavior unable
    // to participate in a db transaction, because by the time TransactionBehavior is executed and a
    // DbConnection is created and stored in AsyncLocal storage, the repos and queryables used in the
    // behavior are already resolved.
    // @@TODO??: Honestly, MediatR has given me so much grief that I should probably just write my own thing.

    public RestrictedAccessBehavior(
        ICurrentPrincipal currentPrincipal,
        Lazy<IWhitelistQueryable> whitelistQueryable
    )
    {
        _currentPrincipal = currentPrincipal;
        _whitelistQueryable = whitelistQueryable;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var authorizeAttributes = request
            .GetType()
            .GetCustomAttributes<RequireAuthorizationAttribute>();

        if (
            authorizeAttributes.Any() &&
            (
                _currentPrincipal.Id == null ||
                !await _whitelistQueryable.Value.CheckIsWhitelisted(_currentPrincipal.Id)
            )
        )
        {
            return new()
            {
                Error = new AuthorizationError("Sorry, the access is currently restricted. Email me at admin@truquest.io to get access")
            };
        }

        return await next();
    }
}
