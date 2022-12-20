#!/usr/bin/env dotnet-script

#r "nuget: Nethereum.Web3, 4.11.0"
#r "c:/chekh/projects/truquest/src/Orchestrator/lib/ContractStorageExplorer/src/bin/debug/net7.0/ContractStorageExplorer.dll"

using Internal;

using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

var contract = ContractFinder.Create()
    .WithLayoutDirectory("c:/chekh/projects/truquest/src/Orchestrator/lib/ContractStorageExplorer/test")
    .WithName("TestContract")
    .DeployedAt("0xcf7ed3acca5a467e9e704c703e8d87f634fb0fc9")
    .OnNetwork("http://localhost:8545/")
    .Find();

var value = await contract
    .WalkStorage()
    .Field("map_string_to_string")
    .AsMapping()
    .Key(new SolString("short key 1"))
    .GetValue<SolString>();

Console.WriteLine(value.Value);

var length = await contract
    .WalkStorage()
    .Field("arr_of_struct_b")
    .AsArrayOfStruct("B")
    .Length();

Console.WriteLine(length.ToString());

length = await contract
    .WalkStorage()
    .Field("arr_of_arr_of_arr_of_string")
    .AsArrayOf<SolArray>()
    .Length();

Console.WriteLine(length.ToString());

length = await contract
    .WalkStorage()
    .Field("arr_of_arr_of_arr_of_string")
    .AsArrayOf<SolArray>()
    .Index(14)
    .AsArrayOf<SolArray>()
    .Length();

Console.WriteLine(length.ToString());

length = await contract
    .WalkStorage()
    .Field("arr_of_arr_of_arr_of_string")
    .AsArrayOf<SolArray>()
    .Index(14)
    .AsArrayOf<SolArray>()
    .Index(3)
    .AsArrayOf<SolString>()
    .Length();

Console.WriteLine(length.ToString());