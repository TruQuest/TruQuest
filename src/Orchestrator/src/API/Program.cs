using System.Text.Json;

using KafkaFlow;

using Application;
using Infrastructure;

using API.BackgroundServices;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var app = CreateWebApplication(args);
        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            var client = new HttpClient();
            var body = await File.ReadAllTextAsync("pg-connector.conf.json");
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8083/connectors");
            request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        });

        app.UseCors();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpsRedirection();
        // app.UseAuthorization();

        app.MapControllers();

        var bus = app.Services.CreateKafkaBus();
        await bus.StartAsync();

        app.Run();
    }

    public static WebApplication CreateWebApplication(string[] args)
    {
        DotNetEnv.Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddControllers()
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

        builder.Services.AddHostedService<ContractEventTracker>();
        builder.Services.AddHostedService<BlockTracker>();

        return builder.Build();
    }
}
