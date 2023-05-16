using System.Reflection;
using System.Transactions;

using MediatR;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application;

public class PublisherWrapper
{
    private readonly IPublisher _publisher;
    private readonly ISharedTxnScope _sharedTxnScope;

    public PublisherWrapper(IPublisher publisher, ISharedTxnScope sharedTxnScope)
    {
        _publisher = publisher;
        _sharedTxnScope = sharedTxnScope;
    }

    public async Task Publish<TNotification>(
        TNotification @event, CancellationToken ct = default
    ) where TNotification : INotification
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

            await _publisher.Publish(@event);

            txnScope.Complete();
        }
        else
        {
            await _publisher.Publish(@event);
        }
    }
}