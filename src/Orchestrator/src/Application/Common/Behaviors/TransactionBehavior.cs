using System.Reflection;
using System.Transactions;

using MediatR;

using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly ISharedTxnScope _sharedTxnScope;

    public TransactionBehavior(ISharedTxnScope sharedTxnScope)
    {
        _sharedTxnScope = sharedTxnScope;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var attr = request.GetType().GetCustomAttributes<ExecuteInTxnAttribute>().SingleOrDefault();
        if (attr != null)
        {
            _sharedTxnScope.Init(attr.ExcludeRepos);

            using var txnScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = attr.IsolationLevel },
                TransactionScopeAsyncFlowOption.Enabled
            );

            var response = await next();

            txnScope.Complete();

            return response;
        }

        return await next();
    }
}