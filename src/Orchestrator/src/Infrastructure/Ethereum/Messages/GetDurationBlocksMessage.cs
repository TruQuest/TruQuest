using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("s_durationBlocks", "uint16")]
public class GetDurationBlocksMessage : FunctionMessage { }
