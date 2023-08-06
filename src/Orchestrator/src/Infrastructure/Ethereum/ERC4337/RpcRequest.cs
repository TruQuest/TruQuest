namespace Infrastructure.Ethereum.ERC4337;

internal class RpcRequest
{
    public string Jsonrpc { get; } = "2.0";
    public required string Method { get; init; }
    public required IEnumerable<object> Params { get; init; }
    public int Id { get; } = 0;
}
