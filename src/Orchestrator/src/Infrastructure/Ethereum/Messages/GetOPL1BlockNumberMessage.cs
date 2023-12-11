using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages;

[Function("number", "uint64")]
public class GetOPL1BlockNumberMessage : FunctionMessage { }
