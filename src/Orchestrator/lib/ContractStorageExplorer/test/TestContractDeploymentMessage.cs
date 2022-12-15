using Nethereum.Contracts;

public class TestContractDeploymentMessage : ContractDeploymentMessage
{
    public TestContractDeploymentMessage() : base("0x" + File.ReadAllText("TestContract.bin")) { }
}