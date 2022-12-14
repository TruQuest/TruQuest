namespace ContractStorageExplorer.SolTypes;

public class SolArray : SolType
{
    public override int SizeBits => 32 * 8;

    protected internal override object ValueObject
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override string HexValue => throw new NotSupportedException();
}