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
using Infrastructure;
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
        Signer = new Signer(AccountProvider);
        BlockchainManipulator = new BlockchainManipulator(_app.Configuration);
        ContractCaller = new ContractCaller(
            _app.Logger,
            _app.Configuration,
            AccountProvider,
            _app.Services.GetRequiredService<UserOperationService>(),
            BlockchainManipulator
        );
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
