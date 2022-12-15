namespace ContractStorageExplorer.DTO;

internal class LayoutDto
{
    public IEnumerable<FieldDto> Storage { get; set; }
    public IDictionary<string, TypeDto> Types { get; set; }
}