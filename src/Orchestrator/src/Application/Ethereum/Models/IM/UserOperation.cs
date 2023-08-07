using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Application.Common.Misc;

namespace Application.Ethereum.Models.IM;

public record UserOperation
{
    public required string Sender { get; init; }
    [JsonConverter(typeof(BigIntegerConverter))]
    public required BigInteger Nonce { get; init; }
    public required string InitCode { get; init; }
    public required string CallData { get; init; }
    [JsonConverter(typeof(BigIntegerConverter))]
    public required BigInteger CallGasLimit { get; init; }
    [JsonConverter(typeof(BigIntegerConverter))]
    public required BigInteger VerificationGasLimit { get; init; }
    [JsonConverter(typeof(BigIntegerConverter))]
    public required BigInteger PreVerificationGas { get; init; }
    [JsonConverter(typeof(BigIntegerConverter))]
    public required BigInteger MaxFeePerGas { get; init; }
    [JsonConverter(typeof(BigIntegerConverter))]
    public required BigInteger MaxPriorityFeePerGas { get; init; }
    public required string PaymasterAndData { get; init; }
    public required string Signature { get; init; }

    public override string ToString() => JsonSerializer.Serialize(
        this,
        new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }
    );
}

public class BigIntegerConverter : JsonConverter<BigInteger>
{
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        BigInteger.Parse(reader.GetString()!.Substring(2), NumberStyles.HexNumber);

    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToByteArray(isUnsigned: true, isBigEndian: true).ToHex(prefix: true));
}
