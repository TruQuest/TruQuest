using System.Reflection;
using System.Transactions;

using MediatR;

using Domain.Results;
using Application.Common.Attributes;

namespace Infrastructure;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var attr = request.GetType().GetCustomAttribute<ExecuteInTxnAttribute>();
        if (attr != null)
        {
            using var txnScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = attr.IsolationLevel },
                TransactionScopeAsyncFlowOption.Enabled
            );

            var response = await next();
            if (response.Error == null) txnScope.Complete();

            return response;
        }

        return await next();
    }
}
