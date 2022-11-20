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
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationBehavior(IAuthorizationService authorizationService)
    {
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

        var error = await _authorizationService.Authorize(authorizeAttributes);
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