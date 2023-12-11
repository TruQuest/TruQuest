using System.Text.Json;

using KafkaFlow;

namespace Infrastructure.Kafka;

internal class MessageSerializer : ISerializer, IDeserializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<object> DeserializeAsync(Stream input, Type type, ISerializerContext context) =>
        (await JsonSerializer.DeserializeAsync(input, type, _options).ConfigureAwait(false))!;

    public async Task SerializeAsync(object message, Stream output, ISerializerContext context) =>
        await JsonSerializer.SerializeAsync(output, message, _options);
}
