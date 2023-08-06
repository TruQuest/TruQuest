using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

using Infrastructure.Ethereum.TypedData;

namespace Infrastructure.Ethereum.Messages;

[Function("getUserOpHash", "bytes32")]
public class GetUserOpHashMessage : FunctionMessage
{
    [Parameter("tuple", "userOp", 1, "UserOperationTd")]
    public UserOperationTd UserOp { get; init; }
}
