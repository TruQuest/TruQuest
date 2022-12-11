using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

using Respawn;
using Respawn.Graph;
using MediatR;

using Domain.Aggregates;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using API;

using Tests.FunctionalTests.Helpers;

namespace Tests.FunctionalTests;

public class Sut : IAsyncLifetime
{
    private WebApplication _app;
    private Respawner _respawner;

    private ClaimsPrincipal? _user;

    public Signer Signer { get; private set; }
    public BlockchainManipulator BlockchainManipulator { get; private set; }
    public ContractCaller ContractCaller { get; private set; }

    public async Task InitializeAsync()
    {
        DotNetEnv.Env.TraversePath().Load();

        var appBuilder = API.Program.CreateWebApplicationBuilder(new string[] { });
        appBuilder.Configuration.AddJsonFile("appsettings.Testing.json", optional: false);
        appBuilder.ConfigureServices();

        using (var scope = appBuilder.Services.BuildServiceProvider().CreateScope())
        {
            await _applyMigrations(scope);

            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await appDbContext.Database.OpenConnectionAsync();

            _respawner = await Respawner
                .CreateAsync(appDbContext.Database.GetDbConnection(), new RespawnerOptions
                {
                    SchemasToInclude = new[] { "truquest", "truquest_events" },
                    TablesToIgnore = new Table[] { "__EFMigrationsHistory", "Tags", "AspNetUsers" },
                    DbAdapter = DbAdapter.Postgres
                });
        }

        _app = appBuilder.Build().ConfigurePipeline();
        await _app.DeployContracts();
        await _app.RegisterDebeziumConnector();

        Signer = new Signer(_app.Configuration);
        BlockchainManipulator = new BlockchainManipulator(_app.Configuration);
        ContractCaller = new ContractCaller(_app.Logger, _app.Configuration, BlockchainManipulator);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task<TResult> ExecWithService<TService, TResult>(
        Func<TService, Task<TResult>> func
    ) where TService : notnull
    {
        using var scope = _app.Services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<TService>();
        var result = await func(service);

        return result;
    }

    public Task StartKafkaBus() => _app.StartKafkaBus();

    public Task StartHostedService<T>() where T : IHostedService =>
        _app.Services
            .GetServices<IHostedService>()
            .OfType<T>()
            .First()
            .StartAsync(CancellationToken.None);

    public Task StopHostedService<T>() where T : IHostedService =>
        _app.Services
            .GetServices<IHostedService>()
            .OfType<T>()
            .First()
            .StopAsync(CancellationToken.None);

    private async Task _applyMigrations(IServiceScope scope)
    {
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await appDbContext.Database.MigrateAsync();

        var userIds = new[]
        {
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
        await appDbContext.SaveChangesAsync();

        var eventDbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        await eventDbContext.Database.MigrateAsync();
    }

    public async Task ResetState()
    {
        using var scope = _app.Services.CreateScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await appDbContext.Database.OpenConnectionAsync();
        await _respawner.ResetAsync(appDbContext.Database.GetDbConnection());

        _user = null;
    }

    public T GetConfigurationValue<T>(string key) => _app.Configuration.GetValue<T>(key)!;

    public void RunAs(string userId, string username)
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new("username", username)
            },
            "Bearer"
        ));
    }

    public void RunAsGuest()
    {
        _user = null;
    }

    public async Task<T> SendRequest<T>(IRequest<T> request)
    {
        using var scope = _app.Services.CreateScope();

        if (_user != null)
        {
            var context = scope.ServiceProvider.GetRequiredService<IAuthenticationContext>();
            context.User = _user;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var result = await mediator.Send(request);

        return result;
    }
}