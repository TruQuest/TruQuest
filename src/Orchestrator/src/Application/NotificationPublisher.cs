using System.Reflection;
using System.Transactions;

using MediatR;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application;

public class NotificationPublisher : INotificationPublisher
{
    private readonly ISharedTxnScope _sharedTxnScope;

    public NotificationPublisher(ISharedTxnScope sharedTxnScope)
    {
        _sharedTxnScope = sharedTxnScope;
    }

    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification @event,
        CancellationToken ct
    )
    {
        var attr = @event.GetType().GetCustomAttribute<ExecuteInTxnAttribute>();
        if (attr != null)
        {
            _sharedTxnScope.Init(attr.ExcludeRepos);

            using var txnScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = attr.IsolationLevel },
                TransactionScopeAsyncFlowOption.Enabled
            );

            foreach (var handler in handlerExecutors)
            {
                await handler.HandlerCallback(@event, ct).ConfigureAwait(false);
            }

            txnScope.Complete();
        }
        else
        {
            foreach (var handler in handlerExecutors)
            {
                await handler.HandlerCallback(@event, ct).ConfigureAwait(false);
            }
        }
    }
}