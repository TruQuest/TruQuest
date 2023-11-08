using System.Reflection;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Errors;

public abstract class BaseError
{
    public override string ToString() => GetType().GetCustomAttribute<ErrorAttribute>()!.Name;
}
