using System.Text.Json.Serialization;

namespace ContractStorageExplorer.DTO;

internal class TypeDto
{
    public string Label { get; set; }
    public string Encoding { get; set; }
    public IEnumerable<FieldDto>? Members { get; set; } = null;
    [JsonPropertyName("numberOfBytes")]
    public string NumberOfBytesStr { get; set; }
    [JsonIgnore]
    public int NumberOfBytes => int.Parse(NumberOfBytesStr);
}