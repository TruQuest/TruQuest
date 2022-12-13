using System.Numerics;
using System.Text.Json.Serialization;

using Nethereum.Hex.HexTypes;

namespace ContractStorageExplorer.DTO;

internal class FieldDto
{
    public string Label { get; set; }

    [JsonPropertyName("slot")]
    public string SlotStr { get; set; }

    [JsonIgnore]
    public HexBigInteger Slot => new(BigInteger.Parse(SlotStr));

    public int Offset { get; set; }
}