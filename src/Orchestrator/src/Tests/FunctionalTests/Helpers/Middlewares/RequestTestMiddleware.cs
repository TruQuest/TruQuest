using GoThataway;

using Domain.Results;
using Application.Common.Interfaces;

namespace Tests.FunctionalTests.Helpers.Middlewares;

public class RequestTestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly IAdditionalApplicationRequestSink _sink;

    public RequestTestMiddleware(IAdditionalApplicationRequestSink sink)
    {
        _sink = sink;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
    {
        var result = await next();
        _sink.Add(request);
        return result;
    }
}
