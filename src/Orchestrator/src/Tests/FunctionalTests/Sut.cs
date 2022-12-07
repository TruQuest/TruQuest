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
using static API.Program;

using Tests.FunctionalTests.Helpers;

namespace Tests.FunctionalTests;

public class Sut
{
    private readonly WebApplication _app;
    private readonly Respawner _respawner;

    private ClaimsPrincipal? _user;

    public Signer Signer { get; }

    public Sut()
    {
        DotNetEnv.Env.TraversePath().Load();

        var appBuilder = CreateWebApplicationBuilder(new string[] { });
        appBuilder.Configuration.AddJsonFile("appsettings.Testing.json", optional: false);
        appBuilder = ConfigureServices(appBuilder);

        using (var scope = appBuilder.Services.BuildServiceProvider().CreateScope())
        {
            _applyMigrations(scope);

            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            appDbContext.Database.OpenConnection();

            _respawner = Respawner
                .CreateAsync(appDbContext.Database.GetDbConnection(), new RespawnerOptions
                {
                    SchemasToInclude = new[] { "truquest", "truquest_events" },
                    TablesToIgnore = new Table[] { "__EFMigrationsHistory", "Tags", "AspNetUsers" },
                    DbAdapter = DbAdapter.Postgres
                })
                .GetAwaiter()
                .GetResult();
        }

        _app = ConfigurePipeline(appBuilder.Build());

        Signer = new Signer(_app.Configuration);
    }

    public async Task<TResult> ExecWithService<TService, TResult>(
        Func<TService, Task<TResult>> func
    ) where TService : notnull
    {
        using var scope = _app.Services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<TService>();
        var result = await func(service);

        return result;
    }

    public void StartHostedService<T>() where T : IHostedService
    {
        _app.Services
            .GetServices<IHostedService>()
            .OfType<T>()
            .First()
            .StartAsync(CancellationToken.None)
            .Wait();
    }

    public void StopHostedService<T>() where T : IHostedService
    {
        _app.Services
            .GetServices<IHostedService>()
            .OfType<T>()
            .First()
            .StopAsync(CancellationToken.None)
            .Wait();
    }

    private void _applyMigrations(IServiceScope scope)
    {
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        appDbContext.Database.Migrate();

        var userIds = new[]
        {
            "0xbF2Ff171C3C4A63FBBD369ddb021c75934005e81",
            "0x3C44CdDdB6a900fa2b585dd299e03d12FA4293BC",
            "0x90F79bf6EB2c4f870365E785982E1f101E93b906",
            "0x15d34AAf54267DB7D7c367839AAf71A00a2C6A65",
            "0x9965507D1a55bcC2695C58ba16FB37d819B0A4dc",
            "0x976EA74026E726554dB657fA54763abd0C3a0aa9",
            "0x14dC79964da2C08b23698B3D3cc7Ca32193d9955",
            "0x23618e81E3f5cdF7f54C3d65f7FBc0aBf5B21E8f",
            "0xa0Ee7A142d267C1f36714E4a8F75612F20a79720",
            "0xBcd4042DE499D14e55001CcbB24a551F3b954096"
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
    }

    public void ResetState()
    {
        using var scope = _app.Services.CreateScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        appDbContext.Database.OpenConnection();
        _respawner.ResetAsync(appDbContext.Database.GetDbConnection()).Wait();

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