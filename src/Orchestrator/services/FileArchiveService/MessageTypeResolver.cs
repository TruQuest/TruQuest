using System.Reflection;
using System.Text;

using KafkaFlow;

internal class MessageTypeResolver : IMessageTypeResolver
{
    public Type OnConsume(IMessageContext context)
    {
        return Assembly.GetExecutingAssembly().GetType(
            $"Messages.Requests.{Encoding.UTF8.GetString(context.Headers["trq.requestType"])}"
        )!;
    }

    public void OnProduce(IMessageContext context)
    {
        context.Headers.SetString("trq.responseType", context.Message.Value.GetType().Name);
    }
}
