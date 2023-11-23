using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Fido2NetLib;
using FluentValidation;

namespace Application;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var fidoConfig = configuration.GetSection("Fido");
        services
            .AddFido2(options =>
            {
                options.ServerDomain = fidoConfig["ServerDomain"]!;
                options.ServerName = fidoConfig["ServerName"]!;
                options.Origins = fidoConfig.GetSection("Origins").Get<HashSet<string>>();
                options.TimestampDriftTolerance = fidoConfig.GetValue<int>("TimestampDriftToleranceMs");
                options.BackupEligibleCredentialPolicy = Fido2Configuration.CredentialBackupPolicy.Allowed;
                options.BackedUpCredentialPolicy = Fido2Configuration.CredentialBackupPolicy.Allowed;
            });

        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly(),
            lifetime: ServiceLifetime.Singleton,
            includeInternalTypes: true
        );

        return services;
    }
}
