using Microsoft.AspNetCore.SignalR;

using Domain.Results;

namespace API.Hubs.Filters;

public class ConvertHandleErrorToHubExceptionFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next
    )
    {
        var handleResult = (HandleResult)(await next(invocationContext))!;
        var error = handleResult.Error;
        if (error != null)
        {
            throw new HubException($"[{error.Type}Error] {error.Errors.Values.First().First()}");
        }

        return handleResult;
    }
}