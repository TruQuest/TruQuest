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
    private readonly ICurrentPrincipal _currentPrincipal;

    public AuthorizationBehavior(
        IAuthorizationService authorizationService,
        ICurrentPrincipal currentPrincipal
    )
    {
        _authorizationService = authorizationService;
        _currentPrincipal = currentPrincipal;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
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

        var span = Telemetry.CurrentActivity!;
        span.SetTag("UserId", _currentPrincipal.Id);

        return await next();
    }
}
