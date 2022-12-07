using System.Text.Json;

using KafkaFlow;

using Application;
using Infrastructure;

using API.BackgroundServices;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();
        var appBuilder = ConfigureServices(CreateWebApplicationBuilder(args));
        var app = ConfigurePipeline(appBuilder.Build());
        app.Run();
    }

    public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args) =>
        WebApplication.CreateBuilder(args);

    public static WebApplicationBuilder ConfigureServices(WebApplicationBuilder builder)
    {
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

        return builder;
    }

    public static WebApplication ConfigurePipeline(WebApplication app)
    {
        using var client = new HttpClient();
        var body = File.ReadAllText("pg-connector.conf.json");
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8083/connectors");
        request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = client.Send(request);
        if (!(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict))
        {
            throw new Exception("Debez");
        }

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
        bus.StartAsync().Wait();

        return app;
    }
}
