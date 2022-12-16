namespace ContractStorageExplorer.SolTypes;

internal class SolStruct : SolType
{
    public override int SizeBits { get; }

    protected internal override object ValueObject
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override string HexValue => throw new NotSupportedException();

    internal SolStruct(int sizeBits)
    {
        SizeBits = sizeBits;
    }
}