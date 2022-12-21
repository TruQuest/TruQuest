#!/usr/bin/env dotnet-script

#r "nuget: Nethereum.Web3, 4.11.0"
#r "nuget: Nethereum.HdWallet, 4.11.0"
#r "c:/chekh/projects/truquest/src/Orchestrator/lib/ContractStorageExplorer/src/bin/debug/net7.0/ContractStorageExplorer.dll"

using Internal;

using Nethereum.HdWallet;
using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

var wallet = new Wallet("atom traffic guard castle father vendor modify sauce rebuild true mixture van", null);
Console.WriteLine(wallet.GetAccount(1).Address);