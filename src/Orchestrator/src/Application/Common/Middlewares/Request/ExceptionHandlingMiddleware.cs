using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Results;
using Domain.Errors;

using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Common.Middlewares.Request;

public class ExceptionHandlingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly ILogger<ExceptionHandlingMiddleware<TRequest, TResponse>> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error trying to handle request: {RequestName}", request.GetType().FullName);
            return new TResponse
            {
                Error = new UnhandledError(
                    ex.Message,
                    Telemetry.CurrentActivity!.TraceId.ToHexString(),
                    isRetryable: false
                )
            };
        }
    }
}
