namespace ContractStorageExplorer;

public class ContractFinder
{
    private string? _layoutDirectoryPath;
    private string? _name;
    private string? _address;
    private string? _rpcUrl;

    public static ContractFinder Create() => new ContractFinder();

    public ContractFinder WithLayoutDirectory(string layoutDirectoryPath)
    {
        _layoutDirectoryPath = layoutDirectoryPath;
        return this;
    }

    public ContractFinder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ContractFinder DeployedAt(string address)
    {
        _address = address;
        return this;
    }

    public ContractFinder OnNetwork(string rpcUrl)
    {
        _rpcUrl = rpcUrl;
        return this;
    }

    public Contract Find()
    {
        if (_layoutDirectoryPath == null || _name == null || _address == null || _rpcUrl == null)
        {
            throw new Exception("Not all fields initialized");
        }

        return new Contract(_layoutDirectoryPath, _name, _address, _rpcUrl);
    }
}
