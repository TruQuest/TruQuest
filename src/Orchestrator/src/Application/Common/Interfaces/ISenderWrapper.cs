using MediatR;

namespace Application.Common.Interfaces;

public interface ISenderWrapper
{
    Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request, CancellationToken ct = default,
        IServiceProvider? serviceProvider = null, string? signalRConnectionId = null,
        bool addToAdditionalSinks = false
    );
}
