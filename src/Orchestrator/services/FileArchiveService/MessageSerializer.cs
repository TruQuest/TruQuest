using System.Text.Json;

using KafkaFlow;

internal class MessageSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<object> DeserializeAsync(Stream input, Type type, ISerializerContext context)
    {
        return (await JsonSerializer.DeserializeAsync(input, type, _options).ConfigureAwait(false))!;
    }

    public async Task SerializeAsync(object message, Stream output, ISerializerContext context)
    {
        await JsonSerializer.SerializeAsync(output, message, _options);
    }
}