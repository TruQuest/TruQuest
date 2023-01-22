using Microsoft.AspNetCore.SignalR;

using Application.Common.Interfaces;

namespace API.Hubs.Filters;

public class AddConnectionIdProviderToMethodInvocationScopeFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next
    )
    {
        var attr = Attribute.GetCustomAttribute(
            invocationContext.HubMethod,
            typeof(AddConnectionIdProviderToMethodInvocationScopeAttribute)
        );
        if (attr != null)
        {
            var connectionIdProvider = invocationContext
                .ServiceProvider
                .GetRequiredService<IConnectionIdProvider>();

            connectionIdProvider.ConnectionId = invocationContext.Context.ConnectionId;
        }

        return await next(invocationContext);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AddConnectionIdProviderToMethodInvocationScopeAttribute : Attribute { }