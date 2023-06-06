using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

[Function("getMaxNonce", "uint256")]
public class GetMaxNonceMessage : FunctionMessage { }