namespace Services;

internal interface IResponseDispatcher
{
    Task ReplyTo(string requestId, object message);
    Task SendAsync(object message, string? key = null);
    void Send(object message, string? key = null);
}