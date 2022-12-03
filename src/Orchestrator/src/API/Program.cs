using System.Text.Json;

using Application;
using Infrastructure;

using API.BackgroundServices;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var app = CreateWebApplication(args);

        app.UseCors();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpsRedirection();
        // app.UseAuthorization();

        app.MapControllers();

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
        builder.Services.AddHostedService<DbEventTracker>();
        builder.Services.AddHostedService<BlockTracker>();

        return builder.Build();
    }
}
