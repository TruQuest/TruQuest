#!/usr/bin/env dotnet-script

#r "nuget: Nethereum.Web3, 4.11.0"
#r "nuget: Nethereum.HdWallet, 4.11.0"
#r "nuget: Dapper, 2.0.123"
#r "nuget: Npgsql, 7.0.1"
#r "c:/chekh/projects/truquest/src/Orchestrator/lib/ContractStorageExplorer/src/bin/debug/net7.0/ContractStorageExplorer.dll"

using Internal;

using System.Data;

using Dapper;
using Npgsql;

public class SubjectQm
{
    public Guid Id { get; }
    public string Name { get; }
    public string Details { get; }
    public int Type { get; }
    public string ImageIpfsCid { get; }
    public string CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }

    public List<TagQm> Tags { get; set; } = new();
}

public class TagQm
{
    public int Id { get; }
    public string Name { get; }
}

var conn = new NpgsqlConnection("Host=localhost;Port=5433;Database=TruQuest;Username=postgres;Password=password;SslMode=Disable;SearchPath=truquest;");
await conn.OpenAsync();

var cache = new Dictionary<Guid, SubjectQm>();

await conn.QueryAsync<SubjectQm, TagQm, SubjectQm>(
    @"
        SELECT s.*, t.*
        FROM
            ""Subjects"" s
                LEFT JOIN
            ""SubjectAttachedTags"" st
                ON s.""Id"" = st.""SubjectId""
                LEFT JOIN
            ""Tags"" t
                ON st.""TagId"" = t.""Id""
        WHERE s.""Id"" = @SubjectId
    ",
    (root, joined) =>
    {
        Console.WriteLine((joined == null).ToString());
        if (!cache.ContainsKey(root.Id))
        {
            cache.Add(root.Id, root);
        }

        var cachedParent = cache[root.Id];

        if (joined != null)
        {
            var children = cachedParent.Tags;
            children.Add(joined);
        }

        return cachedParent;
    },
    param: new
    {
        SubjectId = Guid.Parse("ec8837fa-afc6-4dec-bb61-e98bcbaba434")
    }
);

foreach (var kv in cache)
{
    Console.WriteLine(kv.Value.Name);
    Console.WriteLine(kv.Value.Tags.Count.ToString());
    foreach (var tag in kv.Value.Tags)
    {
        Console.WriteLine(tag.Name);
    }
}