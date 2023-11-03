using Microsoft.Extensions.Logging;

using MediatR;

using Application.Common.Interfaces;
using Application.Common.Models.IM;

namespace Application;

public class SenderWrapper
{
    private readonly ILogger<SenderWrapper> _logger;
    private readonly ISender _sender;
    private readonly IEnumerable<IAdditionalApplicationRequestSink> _additionalSinks;

    public SenderWrapper(
        ILogger<SenderWrapper> logger,
        ISender sender,
        IEnumerable<IAdditionalApplicationRequestSink> additionalSinks
    )
    {
        _logger = logger;
        _sender = sender;
        _additionalSinks = additionalSinks;
    }

    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request, CancellationToken ct = default, bool addToAdditionalSinks = false
    )
    {
        string? traceparent = null;
        if (request is DeferredTaskCommand command) traceparent = command.Traceparent;

        // @@NOTE: Passing null 'traceparent' is the same as not passing it at all, that is,
        // parent gets set from Activity.Current if any.
        using var span = Telemetry.StartActivity(request.GetType().FullName!, traceparent: traceparent)!;

        var response = await _sender.Send(request);

        if (addToAdditionalSinks)
        {
            foreach (var sink in _additionalSinks) await sink.Add(request);
        }

        return response;
    }
}
