using System.Text.Json;

using Nethereum.Web3;

using ContractStorageExplorer.DTO;

namespace ContractStorageExplorer;

public class Contract
{
    internal readonly string _name;
    internal readonly string _address;
    internal readonly string _rpcUrl;
    internal readonly LayoutDto _layout;

    internal readonly Web3 _web3;

    internal Contract(string layoutDirectoryPath, string name, string address, string rpcUrl)
    {
        _name = name;
        _address = address;
        _rpcUrl = rpcUrl;

        _layout = JsonSerializer.Deserialize<LayoutDto>(
            File.ReadAllText(Path.Combine(layoutDirectoryPath, $"{name}_storage.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;

        _web3 = new Web3(_rpcUrl);
    }

    public ContractStorage WalkStorage() => new ContractStorage(this);
}