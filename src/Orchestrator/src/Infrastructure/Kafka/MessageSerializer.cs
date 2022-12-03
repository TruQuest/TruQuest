using System.Text.Json;

using KafkaFlow;

namespace Infrastructure.Kafka;

internal class MessageSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<object> DeserializeAsync(Stream input, Type type, ISerializerContext context)
    {
        return (await JsonSerializer.DeserializeAsync(input, type, _options).ConfigureAwait(false))!;
    }

    public Task SerializeAsync(object message, Stream output, ISerializerContext context)
    {
        throw new NotImplementedException();
    }
}