#!/usr/bin/env dotnet-script

#r "nuget: Nethereum.Web3, 4.11.0"
#r "nuget: Nethereum.HdWallet, 4.11.0"
#r "c:/chekh/projects/truquest/src/Orchestrator/lib/ContractStorageExplorer/src/bin/debug/net7.0/ContractStorageExplorer.dll"
#nullable enable

using Internal;

using System.Net.Http;
using System.Text.Json;
// using Nethereum.HdWallet;
// using ContractStorageExplorer;
// using ContractStorageExplorer.SolTypes;

// var wallet = new Wallet("atom traffic guard castle father vendor modify sauce rebuild true mixture van", null);
// Console.WriteLine(wallet.GetAccount(1).Address);

var filePath1 = "c:/users/chekh/desktop/1.jpg";
var filePath2 = "c:/users/chekh/desktop/2.jpg";

var client = new HttpClient();
var file1 = File.OpenRead(filePath1);
var file2 = File.OpenRead(filePath2);
var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5001/api/v0/add?to-files=/");
var content = new MultipartFormDataContent {
    { new StreamContent(file1), "file1", "alala/" + Path.GetFileName(filePath1) },
    { new StreamContent(file2), "file2", "alala/" + Path.GetFileName(filePath2) },
};
request.Content = content;

var response = await client.SendAsync(request);
if (response.IsSuccessStatusCode)
{
    Console.WriteLine(response.Content.Headers.ContentType!.MediaType);
    using var contentStream = await response.Content.ReadAsStreamAsync();
    using (var contentStreamReader = new StreamReader(contentStream))
    {
        string? line;
        while ((line = await contentStreamReader.ReadLineAsync()) != null)
        {
            Console.WriteLine(line);
            var responseMap = JsonSerializer.Deserialize<Dictionary<string, string>>(line);
            Console.WriteLine(responseMap!["Hash"]);
        }
    }
}
else
{
    Console.WriteLine(response.ReasonPhrase);
}