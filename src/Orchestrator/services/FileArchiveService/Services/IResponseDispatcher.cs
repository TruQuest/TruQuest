namespace Services;

internal interface IResponseDispatcher
{
    Task DispatchFor(string requestId, object message);
}