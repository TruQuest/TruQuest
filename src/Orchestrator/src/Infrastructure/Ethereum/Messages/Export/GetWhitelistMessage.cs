using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages.Export;

[Function("getWhitelist", "address[]")]
public class GetWhitelistMessage : FunctionMessage { }
