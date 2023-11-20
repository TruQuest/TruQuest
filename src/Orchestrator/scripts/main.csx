#!/usr/bin/env dotnet-script

#r "nuget: Nethereum.Web3, 4.11.0"
#r "nuget: Nethereum.HdWallet, 4.11.0"
#r "nuget: Dapper, 2.0.123"
#r "nuget: Npgsql, 7.0.1"
#r "c:/chekh/projects/truquest/src/Orchestrator/lib/ContractStorageExplorer/src/bin/debug/net7.0/ContractStorageExplorer.dll"

using Internal;
// using System.Net.Http;

// var client = new HttpClient();
// client.BaseAddress = new Uri("http://localhost:8080");
// var request = new HttpRequestMessage(HttpMethod.Get, "/ipfs/QmYTECKfqKm9RnBT8wsJbvCNxNEwS9sJ7cpC7EjQVtNAdc");
// var response = await client.SendAsync(request);

// Console.WriteLine(await response.Content.ReadAsStringAsync());



using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

[Function("number", "uint64")]
class GetBlockNumberMessage : FunctionMessage { }

var web3 = new Web3("http://localhost:9545");
var block = await web3.Eth
    .GetContractQueryHandler<GetBlockNumberMessage>()
    .QueryAsync<ulong>(
        "0x4200000000000000000000000000000000000015",
        new GetBlockNumberMessage()
    );

Console.WriteLine(block.ToString());

// using Nethereum.Hex.HexTypes;

// var web3 = new Web3("http://localhost:7545");
// var block = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(
//     new HexBigInteger(296)
// );

// Console.WriteLine(block.Timestamp.Value.ToString());

// using ContractStorageExplorer;
// using ContractStorageExplorer.SolTypes;

// var thingId = Guid.Parse("ef39bb1b-2558-4a95-b274-5b9e4af9290f").ToByteArray();

// var _thingSubmissionVerifierLotteryContract = ContractFinder.Create()
//     .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/layout")
//     .WithName("ThingSubmissionVerifierLottery")
//     .DeployedAt("0x05797936947e92b35438F3fcc0562fDbDA01E6ac")
//     .OnNetwork("http://localhost:7545")
//     .Find();

// var value = await _thingSubmissionVerifierLotteryContract
//     .WalkStorage()
//     .Field("s_participants")
//     .AsMapping()
//     .Key(new SolBytes16(thingId))
//     .AsArrayOf<SolAddress>()
//     .Index(3)
//     .GetValue<SolAddress>();

// Console.WriteLine(value.HexValue);

// using System.Data;

// using Dapper;
// using Npgsql;

// public class SubjectQm
// {
//     public Guid Id { get; }
//     public string Name { get; }
//     public string Details { get; }
//     public int Type { get; }
//     public string ImageIpfsCid { get; }
//     public string CroppedImageIpfsCid { get; }
//     public string SubmitterId { get; }

//     public List<TagQm> Tags { get; set; } = new();
// }

// public class TagQm
// {
//     public int Id { get; }
//     public string Name { get; }
// }

// var conn = new NpgsqlConnection("Host=localhost;Port=5433;Database=TruQuest;Username=postgres;Password=password;SslMode=Disable;SearchPath=truquest;");
// await conn.OpenAsync();

// var cache = new Dictionary<Guid, SubjectQm>();

// await conn.QueryAsync<SubjectQm, TagQm, SubjectQm>(
//     @"
//         SELECT s.*, t.*
//         FROM
//             ""Subjects"" s
//                 LEFT JOIN
//             ""SubjectAttachedTags"" st
//                 ON s.""Id"" = st.""SubjectId""
//                 LEFT JOIN
//             ""Tags"" t
//                 ON st.""TagId"" = t.""Id""
//         WHERE s.""Id"" = @SubjectId
//     ",
//     (root, joined) =>
//     {
//         Console.WriteLine((joined == null).ToString());
//         if (!cache.ContainsKey(root.Id))
//         {
//             cache.Add(root.Id, root);
//         }

//         var cachedParent = cache[root.Id];

//         if (joined != null)
//         {
//             var children = cachedParent.Tags;
//             children.Add(joined);
//         }

//         return cachedParent;
//     },
//     param: new
//     {
//         SubjectId = Guid.Parse("ec8837fa-afc6-4dec-bb61-e98bcbaba434")
//     }
// );

// foreach (var kv in cache)
// {
//     Console.WriteLine(kv.Value.Name);
//     Console.WriteLine(kv.Value.Tags.Count.ToString());
//     foreach (var tag in kv.Value.Tags)
//     {
//         Console.WriteLine(tag.Name);
//     }
// }
