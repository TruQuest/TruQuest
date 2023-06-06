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
    private readonly IEnumerable<IAdditionalContractEventSink> _additionalSinks;

    public PublisherWrapper(
        IPublisher publisher,
        ISharedTxnScope sharedTxnScope,
        IEnumerable<IAdditionalContractEventSink> additionalSinks
    )
    {
        _publisher = publisher;
        _sharedTxnScope = sharedTxnScope;
        _additionalSinks = additionalSinks;
    }

    public async Task Publish<TNotification>(
        TNotification @event, CancellationToken ct = default, bool addToAdditionalSinks = true
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

        if (addToAdditionalSinks)
        {
            foreach (var sink in _additionalSinks)
            {
                await sink.Add(@event);
            }
        }
    }
}