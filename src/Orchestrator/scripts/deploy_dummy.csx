#!/usr/bin/env dotnet-script

#r "nuget: Nethereum.Web3, 4.11.0"

using Internal;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;

public class DummyDeploymentMessage : ContractDeploymentMessage
{
    public static string Bytecode { get; set; }

    public DummyDeploymentMessage() : base(Bytecode) { }
}

var fsr = new FileStream("C:/chekh/Projects/TruQuest/src/dapp/artifacts/Dummy.bin", FileMode.Open, FileAccess.Read);
var sr = new StreamReader(fsr);

var owner = new Account("pk");
var web3 = new Web3(owner, "https://goerli.base.org");

DummyDeploymentMessage.Bytecode = await sr.ReadToEndAsync();

Console.WriteLine(owner.Address);

try
{
    var handler = web3.Eth.GetContractDeploymentHandler<DummyDeploymentMessage>();
    var txnReceipt = await handler.SendRequestAndWaitForReceiptAsync(new()
    {
        MaxPriorityFeePerGas = System.Numerics.BigInteger.Parse("1500000000"),
        MaxFeePerGas = System.Numerics.BigInteger.Parse("1510000000")
    });
    Console.WriteLine($"Deployed Dummy at {txnReceipt.ContractAddress}. Txn hash: {txnReceipt.TransactionHash}");
}
catch (Exception ex)
{
    Console.WriteLine("+++++++++++++++++++++++++++");
    Console.WriteLine(ex.Message);
    Console.WriteLine("+++++++++++++++++++++++++++");
}

