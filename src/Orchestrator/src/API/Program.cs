using System.Text.Json;
using System.Diagnostics;

using Microsoft.AspNetCore.SignalR;

using KafkaFlow;

using Application;
using Application.Common.Interfaces;
using Infrastructure;

using API.BackgroundServices;
using API.Hubs.Filters;
using API.Hubs;
using API.Controllers.Filters;
using API.Hubs.Misc;
using API.Hubs.Clients;

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

        app.Run();
    }

    public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args) =>
        WebApplication.CreateBuilder(args);
}

public static class WebApplicationBuilderExtension
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddControllers(options =>
            {
                options.Filters.Add<ConvertHandleErrorToMvcResponseFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
            options.AddDefaultPolicy(builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            )
        );

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services
            .AddSignalR()
            .AddHubOptions<TruQuestHub>(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(90);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(180);

                options.AddFilter<ConvertHandleErrorToHubExceptionFilter>();
                options.AddFilter<CopyAuthenticationContextToMethodInvocationScopeFilter>();
                // options.AddFilter<AddConnectionIdProviderToMethodInvocationScopeFilter>();
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
        builder.Services.AddSingleton<IClientNotifier, ClientNotifier>();

        // builder.Services.AddScoped<IConnectionIdProvider, ConnectionIdProvider>();

        builder.Services.AddHostedService<ContractEventTracker>();
        builder.Services.AddHostedService<BlockTracker>();

        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseCors();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpsRedirection();
        app.UseAuthentication();

        app.MapControllers();
        app.MapHub<TruQuestHub>("/hub");

        return app;
    }

    public static async Task<WebApplication> DeployContracts(this WebApplication app)
    {
        var processInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            Arguments = "/c cd c:/chekh/projects/truquest/src/dapp && rm -r deployments/ && yarn hardhat deploy",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        var process = new Process
        {
            StartInfo = processInfo
        };
        process.Start();

        app.Logger.LogInformation(await process.StandardOutput.ReadToEndAsync());
        await process.WaitForExitAsync();

        return app;
    }

    public static async Task<WebApplication> RegisterDebeziumConnector(this WebApplication app)
    {
        using var client = new HttpClient();
        var body = await File.ReadAllTextAsync("pg-connector.conf.json");
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8083/connectors");
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
}