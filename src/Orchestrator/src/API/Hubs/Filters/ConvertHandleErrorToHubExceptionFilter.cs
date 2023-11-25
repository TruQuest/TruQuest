using Microsoft.AspNetCore.SignalR;

using Domain.Results;
using Domain.Errors;

namespace API.Hubs.Filters;

public class ConvertHandleErrorToHubExceptionFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next
    )
    {
        var handleResult = (HandleResult)(await next(invocationContext))!;
        var error = handleResult.Error;
        if (error != null) throw new HubException($"{(error is UnhandledError e ? $"[{e.TraceId}] [Unhandled] " : "")}{error.Message}");
        return handleResult;
    }
}
