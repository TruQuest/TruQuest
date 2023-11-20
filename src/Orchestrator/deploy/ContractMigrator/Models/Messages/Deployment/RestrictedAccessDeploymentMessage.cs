using Nethereum.Contracts;

public class RestrictedAccessDeploymentMessage : ContractDeploymentMessage
{
    public static string Bytecode { get; set; }

    public RestrictedAccessDeploymentMessage() : base(Bytecode) { }
}
