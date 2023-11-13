using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace GoThataway;

public interface IRequest<TResponse> { }

public interface IEvent { }

public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

public interface IEventHandler<TEvent> where TEvent : IEvent
{
    Task Handle(TEvent @event, CancellationToken ct);
}

public interface IRequestMiddleware<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct);
}

public interface IEventMiddleware<TEvent> where TEvent : IEvent
{
    Task Handle(TEvent @event, Func<Task> next, CancellationToken ct);
}

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddThataway(
        this IServiceCollection services, Assembly requestsAndEventsAssembly,
        ServiceLifetime lifetime, Action<ThatawayRegistry> configure
    )
    {
        services.Add(new ServiceDescriptor(typeof(Thataway), typeof(Thataway), lifetime));
        var registry = new ThatawayRegistry(requestsAndEventsAssembly, services);
        configure(registry);
        services.AddSingleton(registry);

        return services;
    }
}

public class ThatawayRegistry
{
    private readonly IServiceCollection _services;

    private readonly Dictionary<Type, Type> _requestTypeToHandlerInterfaceType = new();
    private readonly Dictionary<Type, Type> _eventTypeToHandlerInterfaceType = new();
    private readonly Dictionary<Type, List<Type>> _requestTypeToMiddlewareTypes = new();
    private readonly Dictionary<Type, List<Type>> _eventTypeToMiddlewareTypes = new();

    public ThatawayRegistry(Assembly assembly, IServiceCollection services)
    {
        _services = services;
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            Type? interfaceType;
            if (!type.IsAbstract && !type.IsInterface)
            {
                if ((interfaceType = type.GetInterface(typeof(IRequest<>).FullName!)) != null)
                {
                    var responseType = interfaceType.GetGenericArguments().Single();
                    var requestHandlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(type, responseType!);
                    var requestHandlerType = types.Where(t => t.IsAssignableTo(requestHandlerInterfaceType)).Single();
                    services.AddTransient(requestHandlerInterfaceType, requestHandlerType);
                    _requestTypeToHandlerInterfaceType[type] = requestHandlerInterfaceType;
                }
                else if ((interfaceType = type.GetInterface(typeof(IEvent).FullName!)) != null)
                {
                    var eventHandlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(type);
                    var eventHandlerTypes = types.Where(t => t.IsAssignableTo(eventHandlerInterfaceType));
                    foreach (var eventHandlerType in eventHandlerTypes)
                    {
                        services.AddTransient(eventHandlerInterfaceType, eventHandlerType);
                    }
                    _eventTypeToHandlerInterfaceType[type] = eventHandlerInterfaceType;
                }
            }
        }
    }

    public Type GetRequestHandlerInterfaceTypeFor(Type requestType) => _requestTypeToHandlerInterfaceType[requestType];

    public Type GetEventHandlerInterfaceTypeFor(Type eventType) => _eventTypeToHandlerInterfaceType[eventType];

    public IEnumerable<Type> GetMiddlewareTypesForRequest(Type requestType) =>
        _requestTypeToMiddlewareTypes.ContainsKey(requestType) ? _requestTypeToMiddlewareTypes[requestType] : Enumerable.Empty<Type>();

    public IEnumerable<Type> GetMiddlewareTypesForEvent(Type eventType) =>
        _eventTypeToMiddlewareTypes.ContainsKey(eventType) ? _eventTypeToMiddlewareTypes[eventType] : Enumerable.Empty<Type>();

    public void AddRequestMiddleware(Type middlewareType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        // @@TODO: Check that 'middlewareType' is actually a request middleware type.
        _services.Add(new ServiceDescriptor(middlewareType, middlewareType, lifetime));
        foreach (var requestType in _requestTypeToHandlerInterfaceType.Keys)
        {
            var responseType = requestType.GetInterface(typeof(IRequest<>).FullName!)!.GetGenericArguments().Single();
            if (!_requestTypeToMiddlewareTypes.ContainsKey(requestType)) _requestTypeToMiddlewareTypes[requestType] = new();
            _requestTypeToMiddlewareTypes[requestType].Insert(0, middlewareType.MakeGenericType(requestType, responseType));
        }
    }

    public void AddEventMiddleware(Type middlewareType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        // @@TODO: Check that 'middlewareType' is actually an event middleware type.
        _services.Add(new ServiceDescriptor(middlewareType, middlewareType, lifetime));
        foreach (var eventType in _eventTypeToHandlerInterfaceType.Keys)
        {
            if (!_eventTypeToMiddlewareTypes.ContainsKey(eventType)) _eventTypeToMiddlewareTypes[eventType] = new();
            _eventTypeToMiddlewareTypes[eventType].Insert(0, middlewareType.MakeGenericType(eventType));
        }
    }
}

public class Thataway
{
    private readonly ThatawayRegistry _registry;
    private readonly IServiceProvider _serviceProvider;

    public Thataway(ThatawayRegistry registry, IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    private Func<Task<TResponse>> _getRequestHandler<TResponse>(IRequest<TResponse> request, CancellationToken ct)
    {
        return async () =>
        {
            var t = _registry.GetRequestHandlerInterfaceTypeFor(request.GetType());
            dynamic handler = _serviceProvider.GetRequiredService(t);
            return (TResponse)await handler.Handle((dynamic)request, ct);
        };
    }

    private Func<Task<TResponse>> _getRequestMiddleware<TResponse>(
        Type middlewareType, IRequest<TResponse> request, Func<Task<TResponse>> next, CancellationToken ct
    )
    {
        return async () =>
        {
            dynamic middleware = _serviceProvider.GetRequiredService(middlewareType);
            return (TResponse)await middleware.Handle((dynamic)request, next, ct);
        };
    }

    private Func<Task> _getEventHandler(IEvent @event, CancellationToken ct, bool invokeHandlersInParallel)
    {
        return async () =>
        {
            var t = _registry.GetEventHandlerInterfaceTypeFor(@event.GetType());
            IEnumerable<object> handlers = _serviceProvider.GetServices(t)!;
            dynamic e = @event;
            if (invokeHandlersInParallel)
            {
                await Task.WhenAll(handlers.Select((dynamic h) => (Task)h.Handle(e, ct)));
            }
            else
            {
                foreach (dynamic handler in handlers) await handler.Handle(e, ct);
            }
        };
    }

    private Func<Task> _getEventMiddleware(
        Type middlewareType, IEvent @event, Func<Task> next, CancellationToken ct
    )
    {
        return async () =>
        {
            dynamic middleware = _serviceProvider.GetRequiredService(middlewareType);
            await middleware.Handle((dynamic)@event, next, ct);
        };
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default(CancellationToken))
    {
        Func<Task<TResponse>> next = _getRequestHandler(request, ct);
        foreach (var type in _registry.GetMiddlewareTypesForRequest(request.GetType()))
        {
            next = _getRequestMiddleware(type, request, next, ct);
        }

        return next();
    }

    public Task Dispatch(IEvent @event, CancellationToken ct = default(CancellationToken), bool invokeHandlersInParallel = false)
    {
        Func<Task> next = _getEventHandler(@event, ct, invokeHandlersInParallel);
        foreach (var type in _registry.GetMiddlewareTypesForEvent(@event.GetType()))
        {
            next = _getEventMiddleware(type, @event, next, ct);
        }

        return next();
    }
}
