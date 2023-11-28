using System.Reflection;
using System.Text;

using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;

internal class MessageTypeResolver : IMessageTypeResolver
{
    public ValueTask<Type> OnConsumeAsync(IMessageContext context)
    {
        return ValueTask.FromResult(Assembly.GetExecutingAssembly().GetType(
            $"Messages.Requests.{Encoding.UTF8.GetString(context.Headers["trq.requestType"])}"
        )!);
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        context.Headers.SetString("trq.responseType", context.Message.Value.GetType().Name);
        return ValueTask.CompletedTask;
    }
}
