using System.Text;
using System.Reflection;
using System.Text.Json;

using KafkaFlow;

using Application.Common.Interfaces;

namespace Infrastructure.Kafka;

internal class ResponseConsumer : IMessageMiddleware
{
    private readonly IRequestDispatcher _requestDispatcher;
    private readonly Assembly _responseMessagesAssembly;
    private readonly string _responseMessagesNamespace;
    private readonly JsonSerializerOptions _options;

    public ResponseConsumer(IRequestDispatcher requestDispatcher)
    {
        _requestDispatcher = requestDispatcher;
        _responseMessagesAssembly = Assembly.GetAssembly(typeof(IRequestDispatcher))!;
        _responseMessagesNamespace = "Application.Common.Messages.Responses.";
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var requestId = Encoding.UTF8.GetString(context.Headers["requestId"]);
        var responseType = _responseMessagesAssembly.GetType(
            _responseMessagesNamespace + Encoding.UTF8.GetString(context.Headers["responseType"])
        )!;

        var messageBytes = (byte[])context.Message.Value;
        var message = JsonSerializer.Deserialize(messageBytes, responseType, _options)!;
        _requestDispatcher.SetResponseFor(requestId, message);

        await next(context);
    }
}