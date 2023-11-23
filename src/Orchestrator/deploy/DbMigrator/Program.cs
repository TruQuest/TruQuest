using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

using Npgsql;
using Renci.SshNet;
using Renci.SshNet.Common;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using Application.Common.Interfaces;
using Infrastructure.Ethereum;
using Infrastructure.Persistence;
using API;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
if (environment == "Development")
{
    var builder = API.Program.CreateWebApplicationBuilder(new string[] { });
    builder.Host.UseDefaultServiceProvider(options => options.ValidateOnBuild = false);
    var app = builder.ConfigureServices().Build();

    using var scope = app.Services.CreateScope();

    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDbContext.Database.MigrateAsync();
    var dbConn = (NpgsqlConnection)appDbContext.Database.GetDbConnection();
    await dbConn.OpenAsync();
    await dbConn.ReloadTypesAsync();

    using var fs = new FileStream("C:/chekh/Projects/TruQuest/src/Orchestrator/deploy/ContractMigrator/artifacts/contract_addresses.json", FileMode.Open, FileAccess.Read);
    var contractNameToAddress = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);

    var version = app.Configuration["Ethereum:Domain:Version"]!;

    appDbContext.ContractAddresses.AddRange(
        contractNameToAddress!.Select(kv => new ContractAddress(kv.Key, version, kv.Value))
    );
    await appDbContext.SaveChangesAsync();

    var accountProvider = scope.ServiceProvider.GetRequiredService<AccountProvider>();
    var contractCaller = scope.ServiceProvider.GetRequiredService<IContractCaller>();

    Dictionary<string, string> accountNameToUserId = new()
    {
        ["Submitter"] = "615170f7-760f-4383-9276-c3462387945e",
        ["Proposer"] = "1c8f8397-bfbf-44f9-9231-3f5865178647",
        ["Verifier1"] = "46959055-c4dc-47f5-8d9d-4109b2fca208",
        ["Verifier2"] = "02433e23-f818-4417-b7ca-519dadf78447",
        ["Verifier3"] = "c24e6ebc-6784-486e-97aa-5759a27e52bd",
        ["Verifier4"] = "327988f5-64c4-4f35-a083-9f9ef4e68648",
        ["Verifier5"] = "cf86c463-3432-4e4e-ab09-f43c27c3b298",
        ["Verifier6"] = "8777cd9c-122a-4f49-bba0-9f366654a5c4",
    };

    foreach (var kv in accountNameToUserId)
    {
        appDbContext.Users.Add(new User
        {
            Id = kv.Value,
            UserName = accountProvider.GetAccount(kv.Key).Address,
            NormalizedUserName = accountProvider.GetAccount(kv.Key).Address.ToUpper(),
            WalletAddress = await contractCaller.GetWalletAddressFor(accountProvider.GetAccount(kv.Key).Address)
        });
    }
    await appDbContext.SaveChangesAsync();

    foreach (var kv in accountNameToUserId)
    {
        appDbContext.UserClaims.AddRange(new IdentityUserClaim<string>[]
        {
            new()
            {
                UserId = kv.Value,
                ClaimType = "signer_address",
                ClaimValue = accountProvider.GetAccount(kv.Key).Address
            },
            new()
            {
                UserId = kv.Value,
                ClaimType = "wallet_address",
                ClaimValue = await contractCaller.GetWalletAddressFor(accountProvider.GetAccount(kv.Key).Address)
            }
        });

        appDbContext.Whitelist.Add(new WhitelistEntry(
            WhitelistEntryType.SignerAddress, accountProvider.GetAccount(kv.Key).Address
        ));
    }

    var emailsToWhitelist = new[]
    {
        "submitter@gmail.com",
        "proposer@gmail.com",
        "verifier1@gmail.com",
        "verifier2@gmail.com",
        "verifier3@gmail.com",
        "verifier4@gmail.com",
        "verifier5@gmail.com",
        "verifier6@gmail.com",
    };
    appDbContext.Whitelist.AddRange(emailsToWhitelist.Select(e => new WhitelistEntry(WhitelistEntryType.Email, e)));

    appDbContext.Tags.AddRange(
        new Tag("Politics"), new Tag("Sport"), new Tag("IT"),
        new Tag("Film"), new Tag("Music"), new Tag("Show business"),
        new Tag("Writing"), new Tag("Space"), new Tag("Engineering"),
        new Tag("Environment"), new Tag("Technology"), new Tag("Education")
    );
    await appDbContext.SaveChangesAsync();

    var details = await File.ReadAllTextAsync("dummy_details.json");

    var submitterId = accountNameToUserId["Submitter"];

    var subject1 = new Subject(
        name: "Erik \"Magneto\" Lehnsherr",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "QmR1RQALYXRc5RTQCsRChRmDLsYpiwAULFHgYZQvKx26yB",
        croppedImageIpfsCid: "Qme3Rfg9HWVnriG7CJesz5FQtTmDi3zEfSirsr796qGbB7",
        submitterId: submitterId
    );
    subject1.AddTags(new[] { 1, 2, 3 });

    var subject2 = new Subject(
        name: "Sergei Bobrovsky",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "QmQUv5YspkQNKmDvVVPLckd7q1M6uRAjr3k8bCqUNHAyXh",
        croppedImageIpfsCid: "QmTKQdDceyrKFGNoChD3SwzPVQSbiKxB25xHRNN4BNaBs1",
        submitterId: submitterId
    );
    subject2.AddTags(new[] { 4, 5, 6, 8 });

    var subject3 = new Subject(
        name: "Obi-Wan \"Ben\" Kenobi",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "QmUrfj3cjsxgM3yrUJeYYySzQjujv6WvnLAN7fTSU1vRZf",
        croppedImageIpfsCid: "QmXwUWK4TJWQewY9mJr1rZZWSv4iwi8qUdyzroZ6xnuMXS",
        submitterId: submitterId
    );
    subject3.AddTags(new[] { 8, 9 });

    var subject4 = new Subject(
        name: "Kimi Raikkonen",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "QmeepXj5HL82S2LTiFkW12ADN8ejmzffr44PFCujhUPd1C",
        croppedImageIpfsCid: "QmZmHtXnA8R7n8yAwm13tP6CHB2VmCbptNuS94Nn23E7CH",
        submitterId: submitterId
    );
    subject4.AddTags(new[] { 5, 11, 10, 7, 1 });

    var subject5 = new Subject(
        name: "Wrex",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "QmdyQYE7CmEH1x286g3FFtrtUT2mTdmLSZAQuPsVuyoscC",
        croppedImageIpfsCid: "QmdRiVi4fbfZiU1FynewAaJmey6utvqc84CBHxSPVk9Hm5",
        submitterId: submitterId
    );
    subject5.AddTags(new[] { 6, 7, 9 });

    var subject6 = new Subject(
        name: "Dwight Eisenhower",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "QmUPsyycjkfDCge396ksanUTjqtFb9vDkoSodxAnrYTV84",
        croppedImageIpfsCid: "Qmb1F4P1PdP7RrPsLaCksFEmJiNBkQwAdmVeYcMNhsFXow",
        submitterId: submitterId
    );
    subject6.AddTags(new[] { 1 });

    var subject7 = new Subject(
        name: "Scooby Doo",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "Qmceg6fo8MYJvfdGj949gsBT5bEa3YqhJbsfNdFnaT5yJ8",
        croppedImageIpfsCid: "QmTbCPVq4NHM69zwSEizA31tw4ZpZpgfq6YTeoKHcuW5xW",
        submitterId: submitterId
    );
    subject7.AddTags(new[] { 4, 5, 8, 12 });

    var subject8 = new Subject(
        name: "Lightning McQueen",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "QmWtmJcjpTrAyfMU6nnEe8RH9mY8i53QKzzuMGSSNwq3u2",
        croppedImageIpfsCid: "QmcKDq1N24vSd1Vu78h3xuzNaePkpLWB3YCXeV5nAXThky",
        submitterId: submitterId
    );
    subject8.AddTags(new[] { 1, 2, 3 });

    var subject9 = new Subject(
        name: "Waxillium \"Wax\" Ladrian",
        details: details,
        type: SubjectType.Person,
        imageIpfsCid: "QmTkvLaCiyFJFGw25SgqAgbmEQ21PpaJuV5Chw8Bzjfb9A",
        croppedImageIpfsCid: "QmdBVq8JRddJrrT4scU3tYUpkwDEAyUXKZDHErKgkszNxN",
        submitterId: submitterId
    );
    subject9.AddTags(new[] { 6, 10 });

    appDbContext.Subjects.AddRange(new[]
    {
        subject1, subject2, subject3,
        subject4, subject5, subject6,
        subject7, subject8, subject9
    });
    await appDbContext.SaveChangesAsync();

    var thing1 = new Thing(
        id: Guid.NewGuid(),
        title: "Promised to never switch sides",
        details: details,
        imageIpfsCid: "QmXQRH7TshkUd4MUHqMvnwsjcsNoWWV4DN2rYqu6GAaVwm",
        croppedImageIpfsCid: "QmbEqDnJ3nWd61bg1AYNZ127eujC7jZTey7Y4fVmbPBdXs",
        submitterId: submitterId,
        subjectId: subject1.Id!.Value
    );
    thing1.AddEvidence(new[]
    {
        new ThingEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new ThingEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new ThingEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    thing1.AddTags(new[] { 5, 9 });
    thing1.SetState(ThingState.AwaitingFunding);

    var thing2 = new Thing(
        id: Guid.NewGuid(),
        title: "Promised to always look cool",
        details: details,
        imageIpfsCid: "Qmf544CedHScDcJuCBiEkAF4UbQsL6pWfokjX3HTVGK2Yx",
        croppedImageIpfsCid: "QmWEpVtTbck8KGmkDwSRzU2TCbUwztpgaKvTXU878mpm98",
        submitterId: submitterId,
        subjectId: subject1.Id!.Value
    );
    thing2.AddEvidence(new[]
    {
        new ThingEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new ThingEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new ThingEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    thing2.AddTags(new[] { 7, 9, 3 });
    thing2.SetState(ThingState.AwaitingFunding);

    var thing3 = new Thing(
        id: Guid.NewGuid(),
        title: "Said he would lead the mutants into a better future",
        details: details,
        imageIpfsCid: "QmbCPKWZ4v4dTMaEcJPkUgHDcM7HHzwChBKvnzQqqDFW6d",
        croppedImageIpfsCid: "QmZdDdE73rEcXQLK8akMj6pnYChDUVRNPhCyuxoKrG3iDF",
        submitterId: submitterId,
        subjectId: subject1.Id!.Value
    );
    thing3.AddEvidence(new[]
    {
        new ThingEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new ThingEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new ThingEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    thing3.AddTags(new[] { 8 });
    thing3.SetState(ThingState.AwaitingFunding);

    var thing4 = new Thing(
        id: Guid.NewGuid(),
        title: "Vowed to never forget where he came from",
        details: details,
        imageIpfsCid: "QmcDKWcF3BBEmW6rX5zDRPpStetsbf79hCQaAtLcnL1PB7",
        croppedImageIpfsCid: "QmcDKWcF3BBEmW6rX5zDRPpStetsbf79hCQaAtLcnL1PB7",
        submitterId: submitterId,
        subjectId: subject1.Id!.Value
    );
    thing4.AddEvidence(new[]
    {
        new ThingEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new ThingEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new ThingEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    thing4.AddTags(new[] { 10, 9, 5, 2 });
    thing4.SetState(ThingState.AwaitingFunding);

    var thing5 = new Thing(
        id: Guid.NewGuid(),
        title: "Promised to value all life and not kill innocents",
        details: details,
        imageIpfsCid: "QmcSrkMgqtSdzMJEV6yGnpiQ4P3Puia2664DJKDNPBsJPi",
        croppedImageIpfsCid: "QmcSrkMgqtSdzMJEV6yGnpiQ4P3Puia2664DJKDNPBsJPi",
        submitterId: submitterId,
        subjectId: subject1.Id!.Value
    );
    thing5.AddEvidence(new[]
    {
        new ThingEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new ThingEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new ThingEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    thing5.AddTags(new[] { 3, 7 });
    thing5.SetState(ThingState.AwaitingFunding);

    appDbContext.Things.AddRange(new[]
    {
        thing1, thing2, thing3,
        thing4, thing5
    });

    await appDbContext.SaveChangesAsync();

    var proposal1 = new SettlementProposal(
        id: Guid.NewGuid(),
        thingId: thing1.Id,
        title: "Erik switches sides all the time",
        verdict: Verdict.AsGoodAsMaliciousIntent,
        details: details,
        imageIpfsCid: "QmNpfDNHjLWBwBAaQfQFkRED4oN7s6PW5oQ4ucdZJYKUPm",
        croppedImageIpfsCid: "QmNpfDNHjLWBwBAaQfQFkRED4oN7s6PW5oQ4ucdZJYKUPm",
        submitterId: submitterId
    );
    proposal1.AddEvidence(new[]
    {
        new SettlementProposalEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    proposal1.SetState(SettlementProposalState.AwaitingFunding);
    proposal1.SetState(SettlementProposalState.Accepted);

    var proposal2 = new SettlementProposal(
        id: Guid.NewGuid(),
        thingId: thing2.Id,
        title: "Whatever",
        verdict: Verdict.Delivered,
        details: details,
        imageIpfsCid: "QmWAY2H5df72d7wB8Ba9Nb8u5gA2hXgcJTwQRLTMr9zzwY",
        croppedImageIpfsCid: "QmPEvoeyiwNEusbPpbWRDa7TFTsecvwUVrSYob5MabzZLk",
        submitterId: submitterId
    );
    proposal2.AddEvidence(new[]
    {
        new SettlementProposalEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    proposal2.SetState(SettlementProposalState.AwaitingFunding);
    proposal2.SetState(SettlementProposalState.Accepted);

    var proposal3 = new SettlementProposal(
        id: Guid.NewGuid(),
        thingId: thing3.Id,
        title: "Whatever",
        verdict: Verdict.MotionNotAction,
        details: details,
        imageIpfsCid: "QmWAY2H5df72d7wB8Ba9Nb8u5gA2hXgcJTwQRLTMr9zzwY",
        croppedImageIpfsCid: "QmPEvoeyiwNEusbPpbWRDa7TFTsecvwUVrSYob5MabzZLk",
        submitterId: submitterId
    );
    proposal3.AddEvidence(new[]
    {
        new SettlementProposalEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    proposal3.SetState(SettlementProposalState.AwaitingFunding);
    proposal3.SetState(SettlementProposalState.Accepted);

    var proposal4 = new SettlementProposal(
        id: Guid.NewGuid(),
        thingId: thing4.Id,
        title: "Whatever",
        verdict: Verdict.Delivered,
        details: details,
        imageIpfsCid: "QmWAY2H5df72d7wB8Ba9Nb8u5gA2hXgcJTwQRLTMr9zzwY",
        croppedImageIpfsCid: "QmPEvoeyiwNEusbPpbWRDa7TFTsecvwUVrSYob5MabzZLk",
        submitterId: submitterId
    );
    proposal4.AddEvidence(new[]
    {
        new SettlementProposalEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    proposal4.SetState(SettlementProposalState.AwaitingFunding);
    proposal4.SetState(SettlementProposalState.Accepted);

    var proposal5 = new SettlementProposal(
        id: Guid.NewGuid(),
        thingId: thing5.Id,
        title: "Whatever",
        verdict: Verdict.NoEffortWhatsoever,
        details: details,
        imageIpfsCid: "QmWAY2H5df72d7wB8Ba9Nb8u5gA2hXgcJTwQRLTMr9zzwY",
        croppedImageIpfsCid: "QmPEvoeyiwNEusbPpbWRDa7TFTsecvwUVrSYob5MabzZLk",
        submitterId: submitterId
    );
    proposal5.AddEvidence(new[]
    {
        new SettlementProposalEvidence(
            originUrl: "https://stackoverflow.com",
            ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
            previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://twitter.com",
            ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
            previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
        ),
        new SettlementProposalEvidence(
            originUrl: "https://google.com",
            ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
            previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
        ),
    });
    proposal5.SetState(SettlementProposalState.AwaitingFunding);
    proposal5.SetState(SettlementProposalState.Accepted);

    appDbContext.SettlementProposals.AddRange(new[]
    {
        proposal1, proposal2, proposal3,
        proposal4, proposal5
    });
    await appDbContext.SaveChangesAsync();

    thing1.AcceptSettlementProposal(proposal1.Id);
    thing1.SetState(ThingState.Settled);

    thing2.AcceptSettlementProposal(proposal2.Id);
    thing2.SetState(ThingState.Settled);

    thing3.AcceptSettlementProposal(proposal3.Id);
    thing3.SetState(ThingState.Settled);

    thing4.AcceptSettlementProposal(proposal4.Id);
    thing4.SetState(ThingState.Settled);

    thing5.AcceptSettlementProposal(proposal5.Id);
    thing5.SetState(ThingState.Settled);

    await appDbContext.SaveChangesAsync();

    var eventDbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    await eventDbContext.Database.MigrateAsync();
    dbConn = (NpgsqlConnection)eventDbContext.Database.GetDbConnection();
    await dbConn.OpenAsync();
    await dbConn.ReloadTypesAsync();

    eventDbContext.BlockProcessedEvent.Add(new BlockProcessedEvent(id: 1, blockNumber: null));
    await eventDbContext.SaveChangesAsync();
}
else if (environment == "Staging")
{
    using var ms = new MemoryStream();
    using var sw = new StreamWriter(ms);
    await sw.WriteAsync(Environment.GetEnvironmentVariable("BASTION_PRIVATE_KEY"));
    await sw.FlushAsync();
    ms.Seek(0, SeekOrigin.Begin);

    using var keyFile = new PrivateKeyFile(ms);

    using var client = new SshClient(
        Environment.GetEnvironmentVariable("BASTION_HOST"),
        Environment.GetEnvironmentVariable("BASTION_USER"),
        keyFile
    );

    client.Connect();

    Console.WriteLine($"Connected to bastion: {client.IsConnected}");

    var localhost = "127.0.0.1";
    uint localPort = 5432;

    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbPort = uint.Parse(Environment.GetEnvironmentVariable("DB_PORT")!);
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

    using var tunnel = new ForwardedPortLocal(localhost, localPort, dbHost, dbPort);
    client.AddForwardedPort(tunnel);

    tunnel.Exception += (object? _, ExceptionEventArgs e) =>
    {
        Console.WriteLine($"Error: {e.Exception}");
    };

    tunnel.Start();

    Console.WriteLine("Tunnel opened");

    var builder = API.Program.CreateWebApplicationBuilder(new string[]
    {
        "--ConnectionStrings:Postgres",
        $"Host={localhost};Port={localPort};Database={dbName};Username={dbUser};Password={dbPassword};"
    });
    builder.Host.UseDefaultServiceProvider(options => options.ValidateOnBuild = false);
    var app = builder.ConfigureServices().Build();

    Console.WriteLine($"ConnectionString: {app.Configuration["ConnectionStrings:Postgres"]!.Substring(0, 20)}");

    using var scope = app.Services.CreateScope();

    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDbContext.Database.MigrateAsync();
    var dbConn = (NpgsqlConnection)appDbContext.Database.GetDbConnection();
    await dbConn.OpenAsync();
    await dbConn.ReloadTypesAsync();

    using var fs = new FileStream("artifacts/contract_addresses.json", FileMode.Open, FileAccess.Read);
    var contractNameToAddress = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);

    var version = Environment.GetEnvironmentVariable("APPLICATION_VERSION")!;

    appDbContext.ContractAddresses.AddRange(
        contractNameToAddress!.Select(kv => new ContractAddress(kv.Key, version, kv.Value))
    );
    await appDbContext.SaveChangesAsync();

    var eventDbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    await eventDbContext.Database.MigrateAsync();
    dbConn = (NpgsqlConnection)eventDbContext.Database.GetDbConnection();
    await dbConn.OpenAsync();
    await dbConn.ReloadTypesAsync();

    if (await eventDbContext.BlockProcessedEvent.CountAsync() == 0)
    {
        eventDbContext.BlockProcessedEvent.Add(new BlockProcessedEvent(id: 1, blockNumber: null));
        await eventDbContext.SaveChangesAsync();
    }

    tunnel.Stop();
    client.Disconnect();

    Console.WriteLine("Tunnel closed and disconnected from bastion");
    Console.WriteLine("Migrations applied");
}
