using System.Reflection;

using MediatR;

using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly IAuthenticationContext _authenticationContext;
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationBehavior(
        IAuthenticationContext authenticationContext,
        IAuthorizationService authorizationService
    )
    {
        _authenticationContext = authenticationContext;
        _authorizationService = authorizationService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        var authorizeAttributes = request
            .GetType()
            .GetCustomAttributes<RequireAuthorizationAttribute>();

        var error = await _authorizationService.Authorize(
            _authenticationContext, authorizeAttributes
        );
        if (error != null)
        {
            return new()
            {
                Error = error
            };
        }

        return await next();
    }
}