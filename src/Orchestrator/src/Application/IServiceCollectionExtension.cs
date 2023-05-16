using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Application.Common.Behaviors;

namespace Application;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.Lifetime = ServiceLifetime.Scoped;
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(AuthorizationBehavior<,>), ServiceLifetime.Scoped);
            config.AddOpenBehavior(typeof(TransactionBehavior<,>), ServiceLifetime.Scoped);
        });
        services.AddScoped<PublisherWrapper>();

        return services;
    }
}