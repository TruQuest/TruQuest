using Nethereum.Contracts;

public class TruthserumDeploymentMessage : ContractDeploymentMessage
{
    public static string Bytecode { get; set; }

    public TruthserumDeploymentMessage() : base(Bytecode) { }
}
