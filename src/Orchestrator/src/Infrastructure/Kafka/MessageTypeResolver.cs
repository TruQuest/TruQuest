using System.Text;
using System.Reflection;

using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;

using Application.Common.Interfaces;

namespace Infrastructure.Kafka;

internal class MessageTypeResolver : IMessageTypeResolver
{
    private readonly Assembly _responseMessagesAssembly;
    private readonly string _responseMessagesNamespace;

    public MessageTypeResolver()
    {
        _responseMessagesAssembly = Assembly.GetAssembly(typeof(IRequestDispatcher))!;
        _responseMessagesNamespace = "Application.Common.Messages.Responses.";
    }

    public ValueTask<Type> OnConsumeAsync(IMessageContext context)
    {
        return ValueTask.FromResult(_responseMessagesAssembly.GetType(
            _responseMessagesNamespace + Encoding.UTF8.GetString(context.Headers["trq.responseType"])
        )!);
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        context.Headers.SetString("trq.requestType", context.Message.Value.GetType().Name);
        return ValueTask.CompletedTask;
    }
}
