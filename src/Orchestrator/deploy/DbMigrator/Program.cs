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
    "0x20FD69D46DC690ef926d209FF016398D6613F168",
    "0x29b9B8924cD0c6eae70981f611f3A2a07AC61f16",
    "0xFC2a6bE9D03eb0F4Db06EaBCac63be3f5002A09B",
    "0x0aB37d130deD0a85fCf2d472ac7aef1650C3CaaE",
    "0x881606962701F9483d1D5FAD45d48C27Ec9698E7",
    "0xaB45E127Fd54B2302E0B1c76d0444b50E12D6d1B",
    "0x297c19fb45f0a4022c6D7030f21696207e51B9B8",
    "0x9914DADEe4De641Da1f124Fc6026535be249ECc8",

    "0x69c2ac462AeeD245Fd1A92C789A5d6ccf94b05B7",
    "0xd5938750a90d2B1529bE082dF1030882DEF5dBab",
    "0x334A60c06D394Eef6970A0A6679DDbE767972FeD",
    "0xcaF234cCb63cd528Aeb67Be009230f7a81563E7a",
    "0x81d7125E7EF2ada9171904760D081cc08510C865",
    "0x5d6E95D3b671aC27cacB2E8E61c3EC23f9C226EC",
    "0x6105C4b563E975AF7E814f31b4f900f0129919e9",
    "0x2a171e640EECA4e9DF7985eB8a80a19b3a0b6276",
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

var submitterId = "0x20FD69D46DC690ef926d209FF016398D6613F168".Substring(2).ToLower();

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
