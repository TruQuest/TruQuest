using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

using Respawn;
using Respawn.Graph;
using MediatR;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Ethereum;
using API;

using Tests.FunctionalTests.Helpers;

namespace Tests.FunctionalTests;

public class Sut : IAsyncLifetime
{
    public class HttpRequestForFileUpload : IDisposable
    {
        public required HttpRequest Request { get; init; }
        public required Action OnDispose { get; init; }

        public void Dispose() => OnDispose();
    }

    private IWebHostEnvironment _hostEnvironment;
    private WebApplication _app;
    private Respawner _respawner;

    private ClaimsPrincipal? _user;

    public ContractEventSink ContractEventSink { get; private set; }
    public AccountProvider AccountProvider { get; private set; }
    public Signer Signer { get; private set; }
    public BlockchainManipulator BlockchainManipulator { get; private set; }
    public ContractCaller ContractCaller { get; private set; }

    public async Task InitializeAsync()
    {
        DotNetEnv.Env.TraversePath().Load();

        var appBuilder = API.Program.CreateWebApplicationBuilder(new string[] { });
        appBuilder.Configuration.AddJsonFile("appsettings.Testing.json", optional: false);
        appBuilder.ConfigureServices();
        ContractEventSink = new ContractEventSink();
        appBuilder.Services.AddSingleton<IAdditionalContractEventSink>(ContractEventSink);

        using (var scope = appBuilder.Services.BuildServiceProvider().CreateScope())
        {
            await _applyMigrations(scope);

            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await appDbContext.Database.OpenConnectionAsync();

            _respawner = await Respawner
                .CreateAsync(appDbContext.Database.GetDbConnection(), new RespawnerOptions
                {
                    SchemasToInclude = new[] { "truquest", "truquest_events" },
                    TablesToIgnore = new Table[]
                    {
                        "__EFMigrationsHistory",
                        "Tags",
                        "AspNetUsers",
                        "AspNetUserClaims",
                        "BlockProcessedEvent"
                    },
                    DbAdapter = DbAdapter.Postgres
                });
        }

        _app = appBuilder.Build().ConfigurePipeline();
        _hostEnvironment = _app.Services.GetRequiredService<IWebHostEnvironment>();

        await _app.DeployContracts();
        await _app.RegisterDebeziumConnector();
        await _app.DepositFunds();

        // @@NOTE: Activity listener gets registered by a hosted service, which during testing we don't
        // actually start, so, instead, we resolve these providers, which accomplishes the same thing.
        _app.Services.GetRequiredService<TracerProvider>();
        _app.Services.GetRequiredService<MeterProvider>();

        AccountProvider = _app.Services.GetRequiredService<AccountProvider>();
        Signer = new Signer(_app.Configuration, AccountProvider);
        BlockchainManipulator = new BlockchainManipulator(_app.Configuration);
        ContractCaller = new ContractCaller(_app.Logger, _app.Configuration, AccountProvider, BlockchainManipulator);
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
        await appDbContext.SaveChangesAsync();

        appDbContext.UserClaims.AddRange(userIds.Select(id => new IdentityUserClaim<string>
        {
            UserId = id.Substring(2).ToLower(),
            ClaimType = "username",
            ClaimValue = id
        }));

        appDbContext.Tags.AddRange(new("Politics"), new("Sport"), new("IT"));
        await appDbContext.SaveChangesAsync();

        var eventDbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        await eventDbContext.Database.MigrateAsync();

        eventDbContext.BlockProcessedEvent.Add(new BlockProcessedEvent(id: 1, blockNumber: null));
        await eventDbContext.SaveChangesAsync();
    }

    public async Task ResetState()
    {
        using var scope = _app.Services.CreateScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await appDbContext.Database.OpenConnectionAsync();
        await _respawner.ResetAsync(appDbContext.Database.GetDbConnection());

        _user = null;

        ContractEventSink.Reset();
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

    private Stream _getFile(string path)
    {
        var fileInfo = _hostEnvironment.ContentRootFileProvider.GetFileInfo(path);
        if (fileInfo.Exists)
        {
            return fileInfo.CreateReadStream();
        }

        throw new FileNotFoundException();
    }

    public HttpRequestForFileUpload PrepareHttpRequestForFileUpload(
        string[] fileNames, params (string Key, string Value)[] formValues
    )
    {
        var boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
        var form = new MultipartFormDataContent(boundary);
        foreach (var fileName in fileNames)
        {
            form.Add(
                new StreamContent(_getFile($"TestData/{fileName}")),
                Path.GetFileNameWithoutExtension(fileName),
                fileName
            );
        }

        foreach (var kv in formValues)
        {
            form.Add(new StringContent(kv.Value), kv.Key);
        }

        var context = new DefaultHttpContext();
        context.Request.Headers.Add("Content-Type", $"multipart/form-data; boundary={boundary}");
        context.Request.Body = form.ReadAsStream();

        return new HttpRequestForFileUpload
        {
            Request = context.Request,
            OnDispose = () => form.Dispose()
        };
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