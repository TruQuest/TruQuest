using System.Security.Claims;

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
using API.BackgroundServices;

using Tests.FunctionalTests.Helpers;

namespace Tests.FunctionalTests;

public class Sut
{
    private static readonly object _sutInitiliazedLock = new();
    private static TaskCompletionSource<Sut>? _sutInitialized;

    public class HttpRequestForFileUpload : IDisposable
    {
        public required HttpRequest Request { get; init; }
        public required Action OnDispose { get; init; }

        public void Dispose() => OnDispose();
    }

    private IWebHostEnvironment _hostEnvironment;
    private WebApplication _app;
    private Respawner _respawner;

    public readonly Dictionary<string, string> AccountNameToUserId = new()
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

    public ApplicationEventChannel ApplicationEventChannel { get; private set; }
    public ApplicationRequestChannel ApplicationRequestChannel { get; private set; }
    public AccountProvider AccountProvider { get; private set; }
    public Signer Signer { get; private set; }
    public BlockchainManipulator BlockchainManipulator { get; private set; }
    public ContractCaller ContractCaller { get; private set; }

    public static async Task<Sut> GetOrInit()
    {
        var mustInit = false;
        TaskCompletionSource<Sut> sutInitialized;
        lock (_sutInitiliazedLock)
        {
            if (_sutInitialized == null)
            {
                _sutInitialized = new();
                mustInit = true;
            }
            sutInitialized = _sutInitialized;
        }

        if (mustInit)
        {
            var sut = new Sut();
            await sut.InitializeAsync();
            sutInitialized.SetResult(sut);

            return sut;
        }

        return await sutInitialized.Task;
    }

    public async Task InitializeAsync()
    {
        DotNetEnv.Env.TraversePath().Load();

        var appBuilder = API.Program.CreateWebApplicationBuilder(new string[] { });
        appBuilder.Configuration.AddJsonFile("appsettings.Testing.json", optional: false);
        appBuilder.ConfigureServices();

        ApplicationEventChannel = new ApplicationEventChannel();
        appBuilder.Services.AddSingleton<IAdditionalApplicationEventSink>(ApplicationEventChannel);
        ApplicationRequestChannel = new ApplicationRequestChannel();
        appBuilder.Services.AddSingleton<IAdditionalApplicationRequestSink>(ApplicationRequestChannel);

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

        await _app.StartKafkaBus();
        await StartHostedService<BlockTracker>();
        await StartHostedService<ContractEventTracker>();
    }

    // @@!!: There is no way to actually call it right now.
    public async Task DisposeAsync()
    {
        await StopHostedService<ContractEventTracker>();
        await StopHostedService<BlockTracker>();
        // @@TODO: Stop Kafka bus.
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

        foreach (var kv in AccountNameToUserId)
        {
            appDbContext.Users.Add(new User
            {
                Id = kv.Value,
                UserName = AccountProvider.GetAccount(kv.Key).Address,
                NormalizedUserName = AccountProvider.GetAccount(kv.Key).Address.ToUpper(),
                WalletAddress = await ContractCaller.GetWalletAddressFor(kv.Key)
            });
        }
        await appDbContext.SaveChangesAsync();

        foreach (var kv in AccountNameToUserId)
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
    }

    public T GetConfigurationValue<T>(string key) => _app.Configuration.GetValue<T>(key)!;

    private Stream _getFile(string path)
    {
        var fileInfo = _hostEnvironment.ContentRootFileProvider.GetFileInfo(path);
        if (fileInfo.Exists) return fileInfo.CreateReadStream();
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

    public async Task<TResponse> SendRequestAs<TResponse>(IRequest<TResponse> request, ClaimsPrincipal? user)
    {
        using var scope = _app.Services.CreateScope();

        if (user != null)
        {
            var context = scope.ServiceProvider.GetRequiredService<IAuthenticationContext>();
            context.User = user;
        }

        var sender = scope.ServiceProvider.GetRequiredService<SenderWrapper>();
        var result = await sender.Send(request);

        return result;
    }
}
