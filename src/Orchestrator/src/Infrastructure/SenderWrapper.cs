using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MediatR;
using Npgsql;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Infrastructure;

public class SenderWrapper
{
    private readonly ILogger<SenderWrapper> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SenderWrapper(
        ILogger<SenderWrapper> logger,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request, CancellationToken ct = default,
        IServiceProvider? serviceProvider = null, string? signalRConnectionId = null
    )
    {
        var attr = request.GetType().GetCustomAttribute<ExecuteInTxnAttribute>();
        if (attr != null)
        {
            int numAttempts = 3; // @@TODO: Config.
            do
            {
                using var scope = _serviceProvider.CreateScope();
                try
                {
                    if (serviceProvider != null)
                    {
                        // @@NOTE: 'serviceProvider' is not null when called by XXXEndpoints or Sut.SendRequest, and
                        // null – when called from Kafka's IMessageHandler.
                        var authenticationContext = scope
                            .ServiceProvider
                            .GetRequiredService<IAuthenticationContext>();

                        var authenticationContextFromRequestScope = serviceProvider
                            .GetRequiredService<IAuthenticationContext>();

                        authenticationContext.User = authenticationContextFromRequestScope.User;
                        authenticationContext.Token = authenticationContextFromRequestScope.Token;
                        authenticationContext.Failure = authenticationContextFromRequestScope.Failure;
                    }
                    if (signalRConnectionId != null)
                    {
                        var connectionIdProvider = scope
                            .ServiceProvider
                            .GetRequiredService<IConnectionIdProvider>();

                        connectionIdProvider.ConnectionId = signalRConnectionId;
                    }

                    var sharedTxnScope = scope.ServiceProvider.GetRequiredService<ISharedTxnScope>();
                    sharedTxnScope.Init();

                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                    return await sender.Send(request);
                }
                catch (PostgresException) // Means txn serialization failure, since ExceptionHandlingBehavior handles everything else.
                {
                    await Task.Delay(500); // @@TODO: Config.
                }
            } while (--numAttempts > 0);

            _logger.LogError(
                "{Request} could not be handled even after {NumAttempts} attempts",
                request.GetType().Name,
                numAttempts
            );

            throw new Exception($"{request.GetType().Name} could not be handled even after {numAttempts} attempts");
        }
        else
        {
            using var scope = _serviceProvider.CreateScope();

            if (serviceProvider != null)
            {
                var authenticationContext = scope
                    .ServiceProvider
                    .GetRequiredService<IAuthenticationContext>();

                var authenticationContextFromRequestScope = serviceProvider
                    .GetRequiredService<IAuthenticationContext>();

                authenticationContext.User = authenticationContextFromRequestScope.User;
                authenticationContext.Token = authenticationContextFromRequestScope.Token;
                authenticationContext.Failure = authenticationContextFromRequestScope.Failure;
            }
            if (signalRConnectionId != null)
            {
                var connectionIdProvider = scope
                    .ServiceProvider
                    .GetRequiredService<IConnectionIdProvider>();

                connectionIdProvider.ConnectionId = signalRConnectionId;
            }

            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            return await sender.Send(request);
        }
    }
}
