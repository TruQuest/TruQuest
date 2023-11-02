using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using FluentValidation;

namespace Application;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly(),
            lifetime: ServiceLifetime.Singleton,
            includeInternalTypes: true
        );

        return services;
    }
}
