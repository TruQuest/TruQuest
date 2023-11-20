using System.Text.Json;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

using Microsoft.AspNetCore.SignalR;

using GoThataway;
using KafkaFlow;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.JsonRpc.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using Fido2NetLib;

using Application;
using Application.Common.Interfaces;
using Application.Common.Middlewares.Request;
using Application.Common.Middlewares.Event;
using Infrastructure;
using Infrastructure.Ethereum;
using Infrastructure.Persistence;

using API.BackgroundServices;
using API.Hubs.Filters;
using API.Hubs;
using API.Hubs.Misc;
using API.Hubs.Clients;
using API.Endpoints;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();

        var app = await CreateWebApplicationBuilder(args)
            .ConfigureServices()
            .Build()
            .ConfigurePipeline()
            .DeployContracts()
                .ContinueWith(deployTask => deployTask.Result.RegisterDebeziumConnector()).Unwrap()
                .ContinueWith(registerTask => registerTask.Result.StartKafkaBus()).Unwrap()
                .ContinueWith(startBusTask => startBusTask.Result.DepositFunds()).Unwrap();

        app.Run();
    }

    public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args) =>
        WebApplication.CreateBuilder(args);
}

public static class WebApplicationBuilderExtension
{
    public static WebApplicationBuilder ConfigureServices(
        this WebApplicationBuilder builder, Action<ThatawayRegistry>? configureThataway = null
    )
    {
        var configuration = builder.Configuration;

        builder.Logging.ClearProviders();
        builder.Logging.AddDebug();
        builder.Logging.AddConsole();

        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();

        builder.Services
            .AddFido2(options =>
            {
                options.ServerDomain = "localhost"; // @@TODO: Config.
                options.ServerName = "TruQuest";
                options.Origins = new HashSet<string>() { "http://localhost:53433" };
                options.TimestampDriftTolerance = 300000;
                options.MDSCacheDirPath = "C:/Users/chekh/Desktop/mds";
                options.BackupEligibleCredentialPolicy = Fido2Configuration.CredentialBackupPolicy.Allowed;
                options.BackedUpCredentialPolicy = Fido2Configuration.CredentialBackupPolicy.Allowed;
            })
            .AddCachedMetadataService(config => // @@TODO: Check if this is necessary.
            {
                config.AddFidoMetadataRepository(delegate { });
            });

        if (!configuration.GetValue<bool>("DbMigrator"))
        {
            Action<ResourceBuilder> configureResource = resource =>
                resource.AddService(
                    serviceName: Telemetry.ServiceName,
                    serviceVersion: "0.1.0",
                    serviceInstanceId: Environment.MachineName
                );

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(configureResource)
                .WithTracing(builder =>
                    builder
                        .AddSource(Telemetry.ActivitySource.Name)
                        .SetSampler(new AlwaysOnSampler()) // @@??: Use this in conjunction with OTEL collector tail sampling?
                        .AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]!);
                        })
                )
                .WithMetrics(builder =>
                    builder
                        .AddMeter(Telemetry.Meter.Name)
                        .AddView(metric =>
                        {
                            if (metric.Name.StartsWith("contract-call."))
                            {
                                return new ExplicitBucketHistogramConfiguration
                                {
                                    Boundaries = new double[]
                                    {
                                        50000,
                                        100000,
                                        150000,
                                        200000,
                                        250000,
                                        300000,
                                        350000,
                                        400000,
                                        450000,
                                        500000
                                    }
                                };
                            }

                            return null;
                        })
                        .AddConsoleExporter(options =>
                            options.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Debug
                        )
                        .AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]!);
                        })
                );

            builder.Logging.AddOpenTelemetry(options =>
            {
                var resourceBuilder = ResourceBuilder.CreateDefault();
                configureResource(resourceBuilder);
                options.SetResourceBuilder(resourceBuilder);

                options.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]!);
                });
            });
        }

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        builder.Services.AddCors(options =>
            options.AddDefaultPolicy(builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            )
        );

        builder.Services.AddThataway(
            requestsAndEventsAssembly: Assembly.GetAssembly(typeof(Application.IServiceCollectionExtension))!,
            ServiceLifetime.Scoped,
            registry =>
            {
                if (configureThataway != null) configureThataway(registry);

                registry.AddRequestMiddleware(typeof(TracingMiddleware<,>));
                registry.AddRequestMiddleware(typeof(ExceptionHandlingMiddleware<,>));
                registry.AddRequestMiddleware(typeof(AuthorizationMiddleware<,>), ServiceLifetime.Transient);
                registry.AddRequestMiddleware(typeof(ValidationMiddleware<,>));
                registry.AddRequestMiddleware(typeof(RequestTransactionMiddleware<,>));
                registry.AddRequestMiddleware(typeof(RestrictedAccessMiddleware<,>), ServiceLifetime.Transient);
                registry.AddRequestMiddleware(typeof(MultipartFormReceivingMiddleware<,>), ServiceLifetime.Transient);

                registry.AddEventMiddleware(typeof(TracingMiddleware<>));
                registry.AddEventMiddleware(typeof(ExceptionLoggingMiddleware<>));
                registry.AddEventMiddleware(typeof(EventTransactionMiddleware<>));
            }
        );

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Environment, builder.Configuration);

        builder.Services
            .AddSignalR()
            .AddHubOptions<TruQuestHub>(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(90);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(180);

                options.AddFilter<ConvertHandleErrorToHubExceptionFilter>();
                options.AddFilter<CopyAuthenticationContextToMethodInvocationScopeFilter>();
                options.AddFilter<AddConnectionIdProviderToMethodInvocationScopeFilter>();
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
        builder.Services.AddSingleton<IClientNotifier, ClientNotifier>();
        builder.Services.AddScoped<IConnectionIdProvider, ConnectionIdProvider>();

        builder.Services.AddHostedService<ContractEventTracker>();
        builder.Services.AddHostedService<BlockTracker>();
        builder.Services.AddHostedService<OrchestratorStatusTracker>();

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseStaticFiles();

        app.UseCors();
        app.UseAuthentication();

        // @@TODO: Figure out how to add endpoint filter to all groups and endpoints at once.
        // app.MapGroup("").AddEndpointFilter(...) doesn't work.
        app.MapUserEndpoints();
        app.MapSubjectEndpoints();
        app.MapThingEndpoints();
        app.MapSettlementProposalEndpoints();
        app.MapGeneralEndpoints();

        app.MapHub<TruQuestHub>("/hub");

        return app;
    }

    public static async Task<WebApplication> DeployContracts(this WebApplication app)
    {
        {
            var processInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments =
                    "/c docker run --rm -v C:/chekh/Projects/TruQuest/src/dapp:/src ethereum/solc:0.8.17" +
                    " --bin --overwrite -o /src/artifacts --base-path /src/contracts --include-path /src/node_modules" +
                    " /src/contracts/Truthserum.sol /src/contracts/RestrictedAccess.sol /src/contracts/TruQuest.sol" +
                    " /src/contracts/ThingValidationVerifierLottery.sol /src/contracts/ThingValidationPoll.sol" +
                    " /src/contracts/SettlementProposalAssessmentVerifierLottery.sol /src/contracts/SettlementProposalAssessmentPoll.sol",
                UseShellExecute = false,
                RedirectStandardError = true
            };
            var process = new Process
            {
                StartInfo = processInfo
            };
            process.Start();

            string? line;
            while ((line = await process.StandardError.ReadLineAsync()) != null)
            {
                app.Logger.LogInformation(line);
            }

            await process.WaitForExitAsync();
        }

        {
            var processInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/c cd C:/chekh/Projects/TruQuest/src/Orchestrator/deploy/ContractMigrator && dotnet run",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = new Process
            {
                StartInfo = processInfo
            };
            process.Start();

            string? line;
            while ((line = await process.StandardOutput.ReadLineAsync()) != null)
            {
                app.Logger.LogInformation(line);
            }

            await process.WaitForExitAsync();
        }

        return app;
    }

    public static async Task<WebApplication> RegisterDebeziumConnector(this WebApplication app)
    {
        using var client = new HttpClient();
        var body = await File.ReadAllTextAsync("pg-connector.conf.json");
        using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8083/connectors");
        request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);
        if (!(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict))
        {
            throw new Exception("Debez");
        }

        return app;
    }

    public static async Task<WebApplication> StartKafkaBus(this WebApplication app)
    {
        var bus = app.Services.CreateKafkaBus();
        await bus.StartAsync();
        return app;
    }

    public static async Task<WebApplication> DepositFunds(this WebApplication app)
    {
        var configuration = app.Configuration;
        var network = configuration["Ethereum:Network"]!;
        var rpcUrl = configuration[$"Ethereum:Networks:{network}:URL"]!;
        var restrictedAccessAddress = configuration[$"Ethereum:Contracts:{network}:RestrictedAccess:Address"]!;
        var truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;

        var accountProvider = app.Services.GetRequiredService<AccountProvider>();

        var users = new[]
        {
            "Submitter",
            "Proposer",
            "Verifier1",
            "Verifier2",
            "Verifier3",
            "Verifier4",
            "Verifier5",
            "Verifier6",
            // "Verifier7",
            // "Verifier8",
            // "Verifier9",
            // "Verifier10",
        };

        var web3 = new Web3(accountProvider.GetAccount("Orchestrator"), rpcUrl);

        var contractCaller = app.Services.GetRequiredService<IContractCaller>();

        var walletAddresses = await Task.WhenAll(
            users.Select(u => contractCaller.GetWalletAddressFor(accountProvider.GetAccount(u).Address))
        );

        await web3.Eth
            .GetContractTransactionHandler<GiveAccessToManyMessage>()
            .SendRequestAndWaitForReceiptAsync(restrictedAccessAddress, new()
            {
                Users = walletAddresses.ToList()
            });

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<MintAndDepositTruthserumToMessage>();
        foreach (var address in walletAddresses)
        {
            var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
                truQuestAddress,
                new()
                {
                    User = address,
                    Amount = BigInteger.Parse("1000000000") // 1 TRU
                }
            );

            if (network == "Ganache")
            {
                await web3.Client.SendRequestAsync(new RpcRequest(Guid.NewGuid().ToString(), "evm_mine"));
            }
        }

        return app;
    }
}

[Function("giveAccessToMany")]
public class GiveAccessToManyMessage : FunctionMessage
{
    [Parameter("address[]", "_users", 1)]
    public List<string> Users { get; set; }
}

[Function("mintAndDepositTruthserumTo")]
public class MintAndDepositTruthserumToMessage : FunctionMessage
{
    [Parameter("address", "_user", 1)]
    public string User { get; init; }
    [Parameter("uint256", "_amount", 2)]
    public BigInteger Amount { get; init; }
}
