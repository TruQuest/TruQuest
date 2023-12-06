using System.Text.Json;
using System.Diagnostics;
using System.Reflection;
using System.Net;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using GoThataway;
using KafkaFlow;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

using Application;
using Application.Common.Interfaces;
using Application.Common.Middlewares.Request;
using Application.Common.Middlewares.Event;
using Infrastructure;
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
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") DotNetEnv.Env.TraversePath().Load();

        var app = await CreateWebApplicationBuilder(args)
            .ConfigureServices()
            .Build()
            .ConfigurePipeline()
            .DeployContracts()
                .ContinueWith(deployTask => deployTask.Result.ApplyDbMigrations()).Unwrap()
                .ContinueWith(applyTask => applyTask.Result.ConfigureContractAddresses()).Unwrap()
                .ContinueWith(configureTask => configureTask.Result.RegisterDebeziumConnector()).Unwrap()
                .ContinueWith(registerTask => registerTask.Result.StartKafkaBus()).Unwrap();

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
        if (builder.Environment.EnvironmentName is "Development" or "Testing")
        {
            builder.Logging.AddDebug();
            builder.Logging.AddConsole();
        }

        Action<ResourceBuilder> configureResource = resource =>
            resource.AddService(
                serviceName: Telemetry.ServiceName,
                serviceVersion: configuration["Ethereum:Domain:Version"]!,
                serviceInstanceId: Environment.MachineName
            );

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(configureResource)
            .WithTracing(builder => builder
                .AddSource(Telemetry.ActivitySource.Name)
                .SetSampler(new AlwaysOnSampler()) // @@??: Use this in conjunction with OTEL collector tail sampling?
                .AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]!))
            )
            .WithMetrics(builder => builder
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
                // .AddConsoleExporter(options => options.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Debug)
                .AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]!))
            );

        builder.Logging.AddOpenTelemetry(options =>
        {
            var resourceBuilder = ResourceBuilder.CreateDefault();
            configureResource(resourceBuilder);
            options.SetResourceBuilder(resourceBuilder);
            options.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]!));
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        if (builder.Environment.EnvironmentName is "Development" or "Testing")
        {
            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(builder =>
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                )
            );
        }

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

        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddInfrastructure(builder.Environment, builder.Configuration);

        builder.Services
            .AddSignalR()
            .AddHubOptions<TruQuestHub>(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(90); // @@TODO: Config.
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

        if (builder.Environment.EnvironmentName is "Staging" && builder.Configuration["DbMigrator"] == null)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownNetworks.Add(new IPNetwork(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(), 16)); // @@??: 24?
            });
        }

        builder.Services.AddHostedService<ContractEventTracker>();
        builder.Services.AddHostedService<BlockTracker>();
        builder.Services.AddHostedService<OrchestratorStatusTracker>();

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.EnvironmentName is "Staging")
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }

        if (app.Environment.EnvironmentName is "Development" or "Testing")
        {
            app.UseCors();
            app.UseStaticFiles();
        }

        app.UseAuthentication();

        app.MapGet("/health", () => Task.FromResult("Ok!")); // @@TODO

        // @@TODO: Figure out how to add endpoint filter to all groups and endpoints at once.
        // app.MapGroup("").AddEndpointFilter(...) doesn't work.
        app.MapUserEndpoints();
        app.MapSubjectEndpoints();
        app.MapThingEndpoints();
        app.MapSettlementProposalEndpoints();
        app.MapGeneralEndpoints();

        app.MapHub<TruQuestHub>("/api/hub");

        return app;
    }

    public static async Task<WebApplication> DeployContracts(this WebApplication app)
    {
        if (app.Environment.EnvironmentName is not ("Development" or "Testing")) return app;

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

    public static async Task<WebApplication> ApplyDbMigrations(this WebApplication app)
    {
        if (app.Environment.EnvironmentName is not ("Development" or "Testing")) return app;

        var processInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            // @@NOTE: We run the dll instead of doing "dotnet run" on DbMigrator, because "dotnet run"ning both the orchestrator and
            // DbMigrator at the same time results in file locking problems.
            Arguments = "/c cd C:/chekh/Projects/TruQuest/src/Orchestrator/deploy/DbMigrator/bin/Debug/net7.0 && dotnet DbMigrator.dll",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        processInfo.EnvironmentVariables.Add("DOTNET_ENVIRONMENT", "Development");

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

    public static async Task<WebApplication> ConfigureContractAddresses(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var version = app.Configuration["Ethereum:Domain:Version"]!;
        var contractAddresses = await appDbContext.ContractAddresses
            .AsNoTracking()
            .Where(ca => ca.Version == version)
            .ToListAsync();

        var network = app.Configuration["Ethereum:Network"]!;

        foreach (var contractAddress in contractAddresses)
        {
            app.Configuration[$"Ethereum:Contracts:{network}:{contractAddress.Name}:Address"] = contractAddress.Address;
        }

        return app;
    }

    public static async Task<WebApplication> RegisterDebeziumConnector(this WebApplication app)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString("Postgres"));
        var payload = await File.ReadAllTextAsync("pg-connector.conf.json");
        payload = payload
            .Replace("${database.hostname}", app.Environment.EnvironmentName is "Development" or "Testing" ? "pg" : builder.Host!)
            .Replace("${database.port}", app.Environment.EnvironmentName is "Development" or "Testing" ? "5432" : builder.Port.ToString())
            .Replace("${database.user}", builder.Username!)
            .Replace("${database.password}", builder.Password!)
            .Replace("${database.dbname}", builder.Database!);

        Exception? e;
        int attempts = 15;
        do
        {
            e = null;
            try
            {
                using var client = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, app.Configuration["Debezium:RegisterEndpoint"]);
                request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                if (!(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict))
                {
                    throw new Exception(
                        $"Error trying to register debezium connector: [{response.StatusCode}] {await response.Content.ReadAsStringAsync()}"
                    );
                }

                app.Logger.LogInformation("Successfully registered debezium connector");
                break;
            }
            catch (Exception ex)
            {
                e = ex;
                app.Logger.LogWarning(ex, "Error trying to register debezium connector");
                await Task.Delay(5000);
            }
        } while (--attempts > 0);

        if (e != null) throw e;

        return app;
    }

    public static async Task<WebApplication> StartKafkaBus(this WebApplication app)
    {
        var bus = app.Services.CreateKafkaBus();
        await bus.StartAsync();
        return app;
    }
}
