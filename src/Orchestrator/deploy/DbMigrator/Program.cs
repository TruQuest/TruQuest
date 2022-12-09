using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Domain.Aggregates;
using Infrastructure.Persistence;
using API;

var app = API.Program.CreateWebApplicationBuilder(new[] { "DbMigrator=true" })
    .ConfigureServices()
    .Build();
using var scope = app.Services.CreateScope();

var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
appDbContext.Database.Migrate();

var userIds = new[] {
    "0x3C44CdDdB6a900fa2b585dd299e03d12FA4293BC",
    "0x90F79bf6EB2c4f870365E785982E1f101E93b906",
    "0x15d34AAf54267DB7D7c367839AAf71A00a2C6A65",
    "0x9965507D1a55bcC2695C58ba16FB37d819B0A4dc",
    "0x976EA74026E726554dB657fA54763abd0C3a0aa9",
    "0x14dC79964da2C08b23698B3D3cc7Ca32193d9955",
    "0x23618e81E3f5cdF7f54C3d65f7FBc0aBf5B21E8f",
    "0xa0Ee7A142d267C1f36714E4a8F75612F20a79720",
    "0xBcd4042DE499D14e55001CcbB24a551F3b954096",

    "0xbF2Ff171C3C4A63FBBD369ddb021c75934005e81",
    "0x529A3efb0F113a2FB6dB0818639EEa26e0661450",
    "0x09f9063bc1355C587F87dE2F7B35740754353Bfb",
    "0x9B7501b9aaa582F0902D100b927AF25809A204ef",
    "0xf4D41175ae91A26311a2B2c49D4eB85CfdDB1898",
    "0xAf73Ad8bd8b023E778b7ccD6Ef490B57adceB655",
    "0x1C0Aa24069f5d9500AC5890195acBB5088BdCcd6",
    "0x202b5E4653846ABB2be555ff09Ba70EeC0AF1451",
    "0xdD5B3fa962aD96590592D4816bb2d025aC0B7225"
};
appDbContext.Users.AddRange(userIds.Select(id => new User
{
    Id = id.Substring(2),
    UserName = id.Substring(20)
}));

appDbContext.Tags.Add(new Tag("Politics"));
appDbContext.SaveChanges();

var eventDbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
eventDbContext.Database.Migrate();