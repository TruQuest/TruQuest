using System.Reflection;

using GoThataway;

using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Monitoring;

namespace Application.Common.Middlewares.Request;

public class AuthorizationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentPrincipal _currentPrincipal;

    public AuthorizationMiddleware(
        IAuthorizationService authorizationService,
        ICurrentPrincipal currentPrincipal
    )
    {
        _authorizationService = authorizationService;
        _currentPrincipal = currentPrincipal;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
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

        Telemetry.CurrentActivity?.SetTag(ActivityTags.UserId, _currentPrincipal.Id);

        return await next();
    }
}
