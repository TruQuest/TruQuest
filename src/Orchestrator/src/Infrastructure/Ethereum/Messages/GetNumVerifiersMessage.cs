using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("s_numVerifiers", "uint8")]
public class GetNumVerifiersMessage : FunctionMessage { }
