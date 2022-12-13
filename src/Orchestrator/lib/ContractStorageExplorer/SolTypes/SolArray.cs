namespace ContractStorageExplorer.SolTypes;

public class SolArray : SolType
{
    public static int SizeBits => 32 * 8;

    public override string HexValue => throw new NotImplementedException();
}