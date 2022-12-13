namespace ContractStorageExplorer;

public class ContractBuilder
{
    private string? _layoutDirectoryPath;
    private string? _name;
    private string? _address;
    private string? _rpcUrl;

    public static ContractBuilder Create() => new ContractBuilder();

    public ContractBuilder WithLayoutDirectory(string layoutDirectoryPath)
    {
        _layoutDirectoryPath = layoutDirectoryPath;
        return this;
    }

    public ContractBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ContractBuilder DeployedAt(string address)
    {
        _address = address;
        return this;
    }

    public ContractBuilder OnNetwork(string rpcUrl)
    {
        _rpcUrl = rpcUrl;
        return this;
    }

    public Contract Build()
    {
        if (_layoutDirectoryPath == null || _name == null || _address == null || _rpcUrl == null)
        {
            throw new Exception("Not all fields initialized");
        }

        return new Contract(_layoutDirectoryPath, _name, _address, _rpcUrl);
    }
}
