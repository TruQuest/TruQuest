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
using Application;
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

    public Dictionary<string, string> _accountNameToUserId = new()
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

    public ApplicationEventSink ApplicationEventSink { get; private set; }
    public ApplicationRequestSink ApplicationRequestSink { get; private set; }
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

        ApplicationEventSink = new ApplicationEventSink();
        appBuilder.Services.AddSingleton<IAdditionalApplicationEventSink>(ApplicationEventSink);
        ApplicationRequestSink = new ApplicationRequestSink();
        appBuilder.Services.AddSingleton<IAdditionalApplicationRequestSink>(ApplicationRequestSink);

        _app = appBuilder.Build().ConfigurePipeline();

        AccountProvider = _app.Services.GetRequiredService<AccountProvider>();
        Signer = new Signer(AccountProvider);
        BlockchainManipulator = new BlockchainManipulator(_app.Configuration);
        ContractCaller = new ContractCaller(
            _app.Logger,
            _app.Configuration,
            AccountProvider,
            _app.Services.GetRequiredService<UserOperationService>(),
            BlockchainManipulator
        );

        using (var scope = _app.Services.CreateScope())
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
                        "AuthCredentials",
                        "BlockProcessedEvent"
                    },
                    DbAdapter = DbAdapter.Postgres
                });
        }

        _hostEnvironment = _app.Services.GetRequiredService<IWebHostEnvironment>();

        await _app.DeployContracts();
        await _app.RegisterDebeziumConnector();
        await _app.DepositFunds();

        // @@NOTE: Activity listener gets registered by a hosted service, which during testing we don't
        // actually start, so, instead, we resolve these providers, which accomplishes the same thing.
        _app.Services.GetRequiredService<TracerProvider>();
        _app.Services.GetRequiredService<MeterProvider>();
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

        appDbContext.Users.AddRange(_accountNameToUserId.Select(kv => new User
        {
            Id = kv.Value,
            UserName = AccountProvider.GetAccount(kv.Key).Address
        }));
        await appDbContext.SaveChangesAsync();

        foreach (var kv in _accountNameToUserId)
        {
            appDbContext.UserClaims.AddRange(new IdentityUserClaim<string>[]
            {
                new()
                {
                    UserId = kv.Value,
                    ClaimType = "signer_address",
                    ClaimValue = AccountProvider.GetAccount(kv.Key).Address
                },
                new()
                {
                    UserId = kv.Value,
                    ClaimType = "wallet_address",
                    ClaimValue = await ContractCaller.GetWalletAddressFor(kv.Key)
                }
            });
        }

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

        ApplicationEventSink.Reset();
        ApplicationRequestSink.Reset();
    }

    public T GetConfigurationValue<T>(string key) => _app.Configuration.GetValue<T>(key)!;

    public void RunAs(string userId, string username)
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                // new("username", username)
            },
            "Bearer"
        ));
    }

    public void RunAs(string accountName)
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new(JwtRegisteredClaimNames.Sub, _accountNameToUserId[accountName]),
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

    public async Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _app.Services.CreateScope();

        if (_user != null)
        {
            var context = scope.ServiceProvider.GetRequiredService<IAuthenticationContext>();
            context.User = _user;
        }

        var sender = scope.ServiceProvider.GetRequiredService<SenderWrapper>();
        var result = await sender.Send(request, serviceProvider: scope.ServiceProvider);

        return result;
    }
}
