using MediatR;
using OpenTelemetry.Trace;

using Domain.Results;

using Application.Common.Errors;

namespace Application.Common.Behaviors;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        var span = Telemetry.StartActivity(request.GetType().Name);
        try
        {
            return await next();
        }
        catch (Exception e)
        {
            span?.RecordException(e);
            return new TResponse
            {
                Error = new ServerError(e.Message)
            };
        }
        finally
        {
            // @@!!: Figure out why it is not null when testing but null when running.
            span?.Dispose();
        }
    }
}