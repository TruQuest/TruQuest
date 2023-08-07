using System.Reflection;

using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum;

internal class AbiEncoder
{
    public string EncodeFunctionData(FunctionMessage message)
    {
        var type = message.GetType();

        var abi = new FunctionABI(type.GetCustomAttribute<FunctionAttribute>()!.Name, false);
        var @params = type
            .GetProperties()
            .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
            .Select(p => (
                Param: p.GetCustomAttribute<ParameterAttribute>()!.Parameter,
                Value: p.GetValue(message)!
            ))
            .OrderBy(p => p.Param.Order);

        abi.InputParameters = @params.Select(p => p.Param).ToArray();

        return new FunctionCallEncoder().EncodeRequest(
            abi.Sha3Signature,
            abi.InputParameters,
            @params.Select(p => p.Value).ToArray()
        );
    }
}
