using System.Text.Json;
using System.Diagnostics;
using System.Numerics;

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

using Domain.Results;
using Domain.Errors;
using Application;
using Application.Common.Interfaces;
using Infrastructure;
using Infrastructure.Ethereum;

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
                .ContinueWith(registerTask => registerTask.Result.StartKafkaBus()).Unwrap();
        // .ContinueWith(startBusTask => startBusTask.Result.DepositFunds()).Unwrap();

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
        app.UseCors();
        app.UseAuthentication();

        async ValueTask<object?> EndpointFilter(
            EndpointFilterInvocationContext context, EndpointFilterDelegate next
        )
        {
            var handleResult = (HandleResult?)await next(context);
            Debug.Assert(handleResult != null);
            if (handleResult.Error != null)
            {
                switch (handleResult.Error)
                {
                    case AuthorizationError:
                        return TypedResults.Json(
                            handleResult,
                            statusCode: handleResult.Error.Errors.Values.First().First() == "Forbidden" ?
                                StatusCodes.Status403Forbidden :
                                StatusCodes.Status401Unauthorized
                        );
                    default:
                        return TypedResults.BadRequest(handleResult);
                }
            }

            return TypedResults.Ok(handleResult);
        }

        // @@TODO: Figure out how to add endpoint filter to all groups and endpoints at once.
        // app.MapGroup("").AddEndpointFilter(...) doesn't work.
        app
            .MapUserEndpoints()
            .AddEndpointFilter(EndpointFilter);

        app
            .MapSubjectEndpoints()
            .AddEndpointFilter(EndpointFilter);

        app
            .MapThingEndpoints()
            .AddEndpointFilter(EndpointFilter);

        app
            .MapSettlementProposalEndpoints()
            .AddEndpointFilter(EndpointFilter);

        app
            .MapGeneralEndpoints()
            .ForEach(endpoint => endpoint.AddEndpointFilter(EndpointFilter));

        app.MapHub<TruQuestHub>("/hub");

        return app;
    }

    public static async Task<WebApplication> DeployContracts(this WebApplication app)
    {
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

        string? line;
        while ((line = await process.StandardOutput.ReadLineAsync()) != null)
        {
            app.Logger.LogInformation(line);
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
            "Verifier7",
            "Verifier8",
            "Verifier9",
            "Verifier10",
        };

        var web3Orchestrator = new Web3(accountProvider.GetAccount("Orchestrator"), rpcUrl);

        var txnDispatcher = web3Orchestrator.Eth.GetContractTransactionHandler<TransferMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            truthserumAddress,
            new()
            {
                To = truQuestAddress,
                Amount = 20000
            }
        );

        if (network == "Ganache")
        {
            await web3Orchestrator.Client.SendRequestAsync(new RpcRequest(Guid.NewGuid().ToString(), "evm_mine"));
        }

        foreach (var user in users)
        {
            txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
                truthserumAddress,
                new()
                {
                    To = accountProvider.GetAccount(user).Address,
                    Amount = 1000
                }
            );

            if (network == "Ganache")
            {
                await web3Orchestrator.Client.SendRequestAsync(new RpcRequest(Guid.NewGuid().ToString(), "evm_mine"));
            }

            var account = accountProvider.GetAccount(user);
            var web3 = new Web3(account, rpcUrl);

            var approveTxnDispatcher = web3.Eth.GetContractTransactionHandler<ApproveMessage>();
            var approveTxnReceipt = await approveTxnDispatcher.SendRequestAndWaitForReceiptAsync(
                truthserumAddress,
                new()
                {
                    Spender = truQuestAddress,
                    Amount = 500
                }
            );

            if (network == "Ganache")
            {
                await web3.Client.SendRequestAsync(new RpcRequest(Guid.NewGuid().ToString(), "evm_mine"));
            }

            var depositTxnDispatcher = web3.Eth.GetContractTransactionHandler<DepositMessage>();
            var depositTxnReceipt = await depositTxnDispatcher.SendRequestAndWaitForReceiptAsync(
                truQuestAddress,
                new()
                {
                    Amount = 500
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

[Function("transfer", "bool")]
public class TransferMessage : FunctionMessage
{
    [Parameter("address", "to", 1)]
    public string To { get; init; }
    [Parameter("uint256", "amount", 2)]
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
