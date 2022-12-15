namespace ContractStorageExplorer.DTO;

internal class TypeDto
{
    public string Label { get; set; }
    public string Encoding { get; set; }
    public IEnumerable<FieldDto>? Members { get; set; } = null;
}