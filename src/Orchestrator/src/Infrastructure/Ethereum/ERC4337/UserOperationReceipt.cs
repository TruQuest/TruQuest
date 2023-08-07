using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Ethereum.ERC4337;

public class GetUserOperationReceiptVm
{
    [JsonConverter(typeof(LongConverter))]
    public required long Nonce { get; init; }
    [JsonConverter(typeof(LongConverter))]
    public required long ActualGasCost { get; init; }
    [JsonConverter(typeof(LongConverter))]
    public required long ActualGasUsed { get; init; }
    public required bool Success { get; init; }
    public String? Reason { get; init; }
    public required UserOperationReceipt Receipt { get; init; }

    public override string ToString() => JsonSerializer.Serialize(
        this,
        new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }
    );
}

public class UserOperationReceipt
{
    public required string BlockHash { get; init; }
    [JsonConverter(typeof(LongConverter))]
    public required long BlockNumber { get; init; }
    public required string TransactionHash { get; init; }
    [JsonConverter(typeof(LongConverter))]
    public required long Confirmations { get; init; }
}

internal class LongConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        long.Parse(reader.GetString()!.Substring(2), NumberStyles.HexNumber);

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);
}
