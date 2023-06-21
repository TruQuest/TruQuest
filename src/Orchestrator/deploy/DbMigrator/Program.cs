using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using Infrastructure.Persistence;
using API;

// using Npgsql;
// using Renci.SshNet;
// using Renci.SshNet.Common;

// using var stream = new MemoryStream();
// using var writer = new StreamWriter(stream);
// writer.Write(Environment.GetEnvironmentVariable("BASTION_PRIVATE_KEY"));
// writer.Flush();
// stream.Position = 0;

// using var keyFile = new PrivateKeyFile(stream);

// using var client = new SshClient(
//     Environment.GetEnvironmentVariable("BASTION_HOST"),
//     Environment.GetEnvironmentVariable("BASTION_USER"),
//     keyFile
// );

// client.Connect();

// var localhost = "127.0.0.1";
// uint localPort = 5432;

// var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
// var dbPort = uint.Parse(Environment.GetEnvironmentVariable("DB_PORT")!);
// var dbName = Environment.GetEnvironmentVariable("DB_NAME");
// var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME");
// var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

// using var tunnel = new ForwardedPortLocal(localhost, localPort, dbHost, dbPort);
// client.AddForwardedPort(tunnel);

// Console.WriteLine($"Connected to bastion: {client.IsConnected}");

// tunnel.Exception += (object? _, ExceptionEventArgs e) =>
// {
//     Console.WriteLine(e.Exception.ToString());
// };

// tunnel.Start();

// using var dbConn = new NpgsqlConnection($"Host={localhost};Port={localPort};Database={dbName};Username={dbUser};Password={dbPassword};");
// dbConn.Open();
// using var cmd = new NpgsqlCommand();
// cmd.Connection = dbConn;
// cmd.CommandText = @"
//     CREATE TABLE IF NOT EXISTS ""TestTest"" (
//         ""Id"" SERIAL PRIMARY KEY,
//         ""Name"" TEXT NOT NULL
//     );
// ";

// cmd.ExecuteNonQuery();

// tunnel.Stop();
// client.Disconnect();

// Console.WriteLine("Migrations applied");

var app = API.Program.CreateWebApplicationBuilder(new[] { "DbMigrator=true" })
    .ConfigureServices()
    .Build();
using var scope = app.Services.CreateScope();

var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
appDbContext.Database.Migrate();

var userIds = new[]
{
    // "0xC7e4C4A64a6EC2821921596770C784580B94b701",
    "0xbF2Ff171C3C4A63FBBD369ddb021c75934005e81",
    "0x529A3efb0F113a2FB6dB0818639EEa26e0661450",
    "0x09f9063bc1355C587F87dE2F7B35740754353Bfb",
    "0x9B7501b9aaa582F0902D100b927AF25809A204ef",
    "0xf4D41175ae91A26311a2B2c49D4eB85CfdDB1898",
    "0xAf73Ad8bd8b023E778b7ccD6Ef490B57adceB655",
    "0x1C0Aa24069f5d9500AC5890195acBB5088BdCcd6",
    "0x202b5E4653846ABB2be555ff09Ba70EeC0AF1451",
    "0xdD5B3fa962aD96590592D4816bb2d025aC0B7225",
    "0x73c26eE1478c96B1ACe803BE249D3949f77A0c7F",
    "0x97F534DeAF3B70d986ac38F7494564583D9f70A2",
    "0xF422334d4C32E72a339F886AC052711279827155",
    "0x230E11E0fc86b48447051E841f2503037E56a0a5",
    "0x2F184C02be71DD94549682460fae534309625e9b",
    "0x06d3DA3948126ee728Bf1975286C2c73788E3fb4",
    "0x733d09dd9Dce5A25ed702Fd7a5502FB16B8461AE",
    "0x0a8eB9AcD21539F211c870A78246b9Bd81a89Efa",
    "0x634Db9D7469f7D8d9c7DfFe484C9FE356Ac23F20",
    "0x236dEe4FA348A1cb1152D54281387fBda3B93F4A",
};
appDbContext.Users.AddRange(userIds.Select(id => new User
{
    Id = id.Substring(2).ToLower(),
    UserName = id
}));
appDbContext.SaveChanges();

appDbContext.UserClaims.AddRange(userIds.Select(id => new IdentityUserClaim<string>
{
    UserId = id.Substring(2).ToLower(),
    ClaimType = "username",
    ClaimValue = id
}));

appDbContext.Tags.AddRange(
    new Tag("Politics"), new Tag("Sport"), new Tag("IT"),
    new Tag("Film"), new Tag("Music"), new Tag("Show business"),
    new Tag("Writing"), new Tag("Space"), new Tag("Engineering"),
    new Tag("Environment"), new Tag("Technology"), new Tag("Education")
);
appDbContext.SaveChanges();

var details = File.ReadAllText("dummy_details.json");

var submitterId = "0xbF2Ff171C3C4A63FBBD369ddb021c75934005e81".Substring(2).ToLower();

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

appDbContext.Subjects.AddRange(new[] {
    subject1, subject2, subject3,
    subject4, subject5, subject6,
    subject7, subject8, subject9
});
appDbContext.SaveChanges();

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
    new Evidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new Evidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new Evidence(
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
    new Evidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new Evidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new Evidence(
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
    new Evidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new Evidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new Evidence(
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
    new Evidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new Evidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new Evidence(
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
    new Evidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new Evidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new Evidence(
        originUrl: "https://google.com",
        ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
        previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
    ),
});
thing5.AddTags(new[] { 3, 7 });
thing5.SetState(ThingState.AwaitingFunding);

appDbContext.Things.AddRange(new[] {
    thing1, thing2, thing3,
    thing4, thing5
});

appDbContext.SaveChanges();

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
    new SupportingEvidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new SupportingEvidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new SupportingEvidence(
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
    new SupportingEvidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new SupportingEvidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new SupportingEvidence(
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
    new SupportingEvidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new SupportingEvidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new SupportingEvidence(
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
    new SupportingEvidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new SupportingEvidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new SupportingEvidence(
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
    new SupportingEvidence(
        originUrl: "https://stackoverflow.com",
        ipfsCid: "QmUJLNPMw9q1kQUrgvYYjBEBBJJjH6cVvg45NuR1588JMq",
        previewImageIpfsCid: "Qmahq4Qdk4BqWTNhNFhRXTtpf6i6JNWDC83owyQkFrhzbt"
    ),
    new SupportingEvidence(
        originUrl: "https://twitter.com",
        ipfsCid: "QmWwZmCgixkQbAXrpJGBg6dKPEP49F7Yw8yUnT4xQLEvK6",
        previewImageIpfsCid: "QmNx8iKKmccFDJLusR9aCJAq6qCKxPRrRRVvVTDhv9KfqS"
    ),
    new SupportingEvidence(
        originUrl: "https://google.com",
        ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
        previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
    ),
});
proposal5.SetState(SettlementProposalState.AwaitingFunding);
proposal5.SetState(SettlementProposalState.Accepted);

appDbContext.SettlementProposals.AddRange(new[] {
    proposal1, proposal2, proposal3,
    proposal4, proposal5
});
appDbContext.SaveChanges();

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

appDbContext.SaveChanges();

var eventDbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
eventDbContext.Database.Migrate();

eventDbContext.BlockProcessedEvent.Add(new BlockProcessedEvent(id: 1, blockNumber: null));
eventDbContext.SaveChanges();
