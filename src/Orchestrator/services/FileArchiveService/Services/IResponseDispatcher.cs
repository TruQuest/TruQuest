namespace Services;

internal interface IResponseDispatcher
{
    Task Reply(string requestId, object message);
    Task Send(string requestId, object message, string? key = null);
    void SendSync(string requestId, object message, string? key = null);
}
