using System.Text.Json;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

using Microsoft.AspNetCore.SignalR;

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
using Application.Common.Behaviors;
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
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
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
                options.ServerDomain = "localhost";
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

        builder.Services.AddMediatR(config =>
        {
            config.Lifetime = ServiceLifetime.Scoped;
            config.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(Application.IServiceCollectionExtension))!);
            config.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>), ServiceLifetime.Singleton);
            config.AddOpenBehavior(typeof(AuthorizationBehavior<,>), ServiceLifetime.Scoped);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>), ServiceLifetime.Singleton);
            config.AddOpenBehavior(typeof(TransactionBehavior<,>), ServiceLifetime.Singleton);
            config.AddOpenBehavior(typeof(MultipartFormReceivingBehavior<,>), ServiceLifetime.Scoped);
        });

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
        // if (!app.Environment.IsDevelopment()) return app;

        var processInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            Arguments = "/c cd c:/chekh/projects/truquest/src/dapp && rm -rf deployments/ && yarn hardhat deploy",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        var process = new Process
        {
            StartInfo = processInfo
        };
        process.Start();

        var network = app.Configuration["Ethereum:Network"]!;

        string? line;
        while ((line = await process.StandardOutput.ReadLineAsync()) != null)
        {
            if (line.StartsWith("deploying"))
            {
                var lineSplit = line.Split(' ');
                var contractName = lineSplit[1].Substring(1, lineSplit[1].Length - 2);
                var contractAddress = lineSplit[lineSplit.Length - 4];
                app.Configuration[$"Ethereum:Contracts:{network}:{contractName}:Address"] = contractAddress;

                app.Logger.LogInformation("Contract {ContractName}: {ContractAddress}", contractName, contractAddress);
            }
            else
            {
                app.Logger.LogInformation(line);
            }
        }

        await process.WaitForExitAsync();

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
        // if (!app.Environment.IsDevelopment()) return app;

        var configuration = app.Configuration;
        var network = configuration["Ethereum:Network"]!;
        var rpcUrl = configuration[$"Ethereum:Networks:{network}:URL"]!;
        var truthserumAddress = configuration[$"Ethereum:Contracts:{network}:Truthserum:Address"]!;
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

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<MintToMessage>();

        var userOperationService = app.Services.GetRequiredService<UserOperationService>();
        var contractCaller = app.Services.GetRequiredService<IContractCaller>();

        foreach (var user in users)
        {
            var account = accountProvider.GetAccount(user);

            var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
                truthserumAddress,
                new()
                {
                    To = await contractCaller.GetWalletAddressFor(account.Address),
                    Amount = BigInteger.Parse("1000000000") // 1 TRU
                }
            );

            if (network == "Ganache")
            {
                await web3.Client.SendRequestAsync(new RpcRequest(Guid.NewGuid().ToString(), "evm_mine"));
            }

            await userOperationService.SendBatch(
                signer: account,
                actions: new()
                {
                    (
                        truthserumAddress,
                        new ApproveMessage
                        {
                            Spender = truQuestAddress,
                            Amount = BigInteger.Parse("500000000") // 0.5 TRU
                        }
                    ),
                    (
                        truQuestAddress,
                        new DepositMessage
                        {
                            Amount = BigInteger.Parse("500000000") // 0.5 TRU
                        }
                    )
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

[Function("mintTo")]
public class MintToMessage : FunctionMessage
{
    [Parameter("address", "_to", 1)]
    public string To { get; init; }
    [Parameter("uint256", "_amount", 2)]
    public BigInteger Amount { get; init; }
}

[Function("approve", "bool")]
public class ApproveMessage : FunctionMessage
{
    [Parameter("address", "spender", 1)]
    public string Spender { get; init; }
    [Parameter("uint256", "amount", 2)]
    public BigInteger Amount { get; init; }
}

[Function("deposit")]
public class DepositMessage : FunctionMessage
{
    [Parameter("uint256", "_amount", 1)]
    public BigInteger Amount { get; init; }
}
