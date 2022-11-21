using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Nethereum.Signer.EIP712;

using Domain.Aggregates;
using Application.Common.Interfaces;

using Infrastructure.Account;
using Infrastructure.Ethereum;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Files;

namespace Infrastructure;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(optionsBuilder =>
            optionsBuilder.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                pgOptionsBuilder => pgOptionsBuilder.MigrationsHistoryTable(
                    "__EFMigrationsHistory", "truquest"
                )
            )
        );

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services
            .AddIdentityCore<User>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -_";
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddDataProtection();
        services.AddHttpContextAccessor();
        // services.TryAddScoped<SignInManager<User>>();

        services.AddScoped<ISharedTxnScope, SharedTxnScope>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IAuthTokenProvider, AuthTokenProvider>();
        services.AddSingleton<Eip712TypedDataSigner>();
        services.AddSingleton<ISigner, Signer>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var rsa = RSA.Create(); // @@NOTE: Important to not dispose.
                rsa.FromXmlString(configuration["JWT:PublicKey"]!);

                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = false; // @@TODO: Depend on environment.
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:Audience"],
                    ValidateLifetime = true, // @@??: false?
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new RsaSecurityKey(rsa)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/ws"))
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var authenticationContext = context
                            .HttpContext
                            .RequestServices
                            .GetRequiredService<IAuthenticationContext>();

                        authenticationContext.User = context.Principal;

                        if (context.Request.Path.StartsWithSegments("/ws"))
                        {
                            authenticationContext.Token = context.SecurityToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var authenticationContext = context
                            .HttpContext
                            .RequestServices
                            .GetRequiredService<IAuthenticationContext>();

                        authenticationContext.Failure = context.Exception;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationCore();
        services.AddScoped<IAuthenticationContext, AuthenticationContext>();
        services.AddTransient<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ICurrentPrincipal, CurrentPrincipal>();

        services.AddSingleton<IFileFetcher, FileFetcher>();
        services.AddSingleton<IImageFetcher, ImageFetcher>();
        services.AddSingleton<IWebPageScreenshotTaker, PlaywrightWebPageScreenshotTaker>();
        services.AddSingleton<IImageSignatureVerifier, ImageSignatureVerifier>();

        services.AddHttpClient("image", (sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var mimeTypes = configuration["Files:Images:AcceptableMimeTypes"]!.Split(',');
            foreach (var mimeType in mimeTypes)
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mimeType));
            }
        });
        services.AddHttpClient("ipfs", (sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(configuration["IPFS:Host"]!);
        });
        services.AddSingleton<IFileStorage, FileStorage>();

        services.AddScoped<ISubjectRepository, SubjectRepository>();

        return services;
    }
}