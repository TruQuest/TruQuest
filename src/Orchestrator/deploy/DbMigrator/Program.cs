using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using Infrastructure.Persistence;
using API;

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
    UserName = id.Substring(2, 20).ToLower()
}));
appDbContext.SaveChanges();

appDbContext.UserClaims.AddRange(userIds.Select(id => new IdentityUserClaim<string>
{
    UserId = id.Substring(2).ToLower(),
    ClaimType = "username",
    ClaimValue = id.Substring(2, 20).ToLower()
}));

appDbContext.Tags.AddRange(new Tag("Politics"), new Tag("Sport"), new Tag("IT"));
appDbContext.SaveChanges();

List<Dictionary<string, object>> details = new()
{
    new()
    {
        ["insert"] = "Hello World!"
    },
    new()
    {
        ["attributes"] = new Dictionary<string, object>()
        {
            ["header"] = 1
        },
        ["insert"] = "\n"
    },
    new()
    {
        ["insert"] = "Welcome to TruQuest!"
    },
    new()
    {
        ["attributes"] = new Dictionary<string, object>()
        {
            ["header"] = 2
        },
        ["insert"] = "\n"
    }
};
var detailsJson = JsonSerializer.Serialize(details);

var submitterId = "0xbF2Ff171C3C4A63FBBD369ddb021c75934005e81".Substring(2).ToLower();

var subject = new Subject(
    name: "Max Venturi",
    details: detailsJson,
    type: SubjectType.Person,
    imageIpfsCid: "QmPt9L8S2vZzomJdwS37hXU6pi3brfd4cThq6e9euNLagz",
    croppedImageIpfsCid: "Qmeg7v9n9L7bHYfwHrdRJPJBoGDXKdeKiEjfZWaW1SGNYg",
    submitterId: submitterId
);
subject.AddTags(new[] { 1, 2, 3 });

appDbContext.Subjects.Add(subject);
appDbContext.SaveChanges();

var thing = new Thing(
    id: Guid.NewGuid(),
    title: "Max promised to behave and not be a little bitch as is his habit in this world",
    details: detailsJson,
    imageIpfsCid: "QmPt9L8S2vZzomJdwS37hXU6pi3brfd4cThq6e9euNLagz",
    croppedImageIpfsCid: "QmeXBT3bk4TYrdGdXfRSG3raSnakvDeSQwS8cFXniFqcbt",
    submitterId: submitterId,
    subjectId: subject.Id!.Value
);
thing.AddEvidence(new[]
{
    new Evidence(
        originUrl: "https://google.com",
        ipfsCid: "QmXKF75UnhR5B7fdhJUNNDC8i7tMrcuJez6MpU1Tv4iMUG",
        previewImageIpfsCid: "QmNysptnFLQ2Ae4YcjHhVuUQDEirb4B4hqgCMAPpCDkbAX"
    )
});
thing.AddTags(new[] { 1, 2, 3 });
thing.SetState(ThingState.AwaitingFunding);

appDbContext.Things.Add(thing);
appDbContext.SaveChanges();

// appDbContext.WatchList.Add(new WatchedItem(
//     submitterId,
//     WatchedItemType.Thing,
//     thing.Id,
//     DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
// ));

// appDbContext.SaveChanges();

var eventDbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
eventDbContext.Database.Migrate();

eventDbContext.BlockProcessedEvent.Add(new BlockProcessedEvent(id: 1, blockNumber: null));
eventDbContext.SaveChanges();
