using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Npgsql;
using MediatR;
using OpenTelemetry.Trace;

using Domain.Results;
using Application;
using Application.Common.Errors;
using Application.Common.Models.IM;

namespace Infrastructure;

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
        string? traceparent = null;
        if (request is DeferredTaskCommand command) traceparent = command.Traceparent;

        // @@NOTE: Passing null 'traceparent' is the same as not passing it at all, that is,
        // parent gets set from Activity.Current if any.
        var span = Telemetry.StartActivity(request.GetType().Name, traceparent: traceparent)!;
        try
        {
            return await next();
        }
        catch (PostgresException ex) when
        (
            ex.SqlState == PostgresErrorCodes.SerializationFailure
        )
        {
            _logger.LogWarning(ex, ex.Message);
            span.RecordException(ex);

            throw;
        }
        catch (DbUpdateException ex) when
        (
            ex.InnerException is PostgresException pgEx &&
            pgEx.SqlState == PostgresErrorCodes.SerializationFailure
        )
        {
            _logger.LogWarning(pgEx, pgEx.Message);
            span.RecordException(pgEx);

            throw pgEx;
        }
        // @@??: Why DbUpdateException is inside an InvalidOperationException ?
        catch (InvalidOperationException ex) when
        (
            ex.InnerException is DbUpdateException dbEx &&
            dbEx.InnerException is PostgresException pgEx &&
            pgEx.SqlState == PostgresErrorCodes.SerializationFailure
        )
        {
            _logger.LogWarning(pgEx, pgEx.Message);
            span.RecordException(pgEx);

            throw pgEx;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ex.Message);
            span.RecordException(ex);

            return new TResponse
            {
                Error = new ServerError(ex.Message)
            };
        }
        finally
        {
            span.Dispose();
        }
    }
}
