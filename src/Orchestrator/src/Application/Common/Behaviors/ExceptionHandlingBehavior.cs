using Microsoft.Extensions.Logging;

using MediatR;
using OpenTelemetry.Trace;

using Domain.Results;

using Application.Common.Errors;

namespace Application.Common.Behaviors;

public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ex.Message);
            Telemetry.CurrentActivity!.RecordException(ex);

            return new TResponse
            {
                Error = new ServerError(ex.Message, isRetryable: false)
            };
        }
    }
}
