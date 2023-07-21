using Microsoft.Extensions.Logging;

using MediatR;
using OpenTelemetry.Trace;

using Domain.Results;

using Application.Common.Errors;

namespace Application.Common.Behaviors;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly ILogger<TracingBehavior<TRequest, TResponse>> _logger;

    public TracingBehavior(ILogger<TracingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

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
            _logger.LogWarning(e, e.Message);
            span.RecordException(e);

            return new TResponse
            {
                Error = new ServerError(e.Message)
            };
        }
        finally
        {
            span!.Dispose();
        }
    }
}
