using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using KafkaFlow;
using KafkaFlow.TypedHandler;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.BlockchainProcessing.ProgressRepositories;

using Domain.Aggregates;
using Domain.Aggregates.Events;
using UserDm = Domain.Aggregates.User;
using Application.Common.Interfaces;

using Infrastructure.User;
using Infrastructure.Ethereum;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Files;
using Infrastructure.Persistence.Repositories.Events;
using Infrastructure.Kafka;
using Infrastructure.Kafka.Messages;
using Infrastructure.Persistence.Queryables;

namespace Infrastructure;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AppDbContext>(optionsBuilder =>
            optionsBuilder
                .UseNpgsql(
                    configuration.GetConnectionString("Postgres") + "SearchPath=truquest;",
                    pgOptionsBuilder => pgOptionsBuilder.MigrationsHistoryTable(
                        "__EFMigrationsHistory", "truquest"
                    )
                )
                .EnableSensitiveDataLogging(environment.IsDevelopment())
        );

        services.AddDbContext<EventDbContext>(optionsBuilder =>
            optionsBuilder
                .UseNpgsql(
                    configuration.GetConnectionString("Postgres") + "SearchPath=truquest_events;",
                    pgOptionsBuilder => pgOptionsBuilder.MigrationsHistoryTable(
                        "__EFMigrationsHistory", "truquest_events"
                    )
                )
                .EnableSensitiveDataLogging(environment.IsDevelopment())
        );

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services
            .AddIdentityCore<UserDm>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -_";
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddDataProtection();
        services.AddHttpContextAccessor();

        services.AddScoped<ISharedTxnScope, SharedTxnScope>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IAuthTokenProvider, AuthTokenProvider>();
        services.AddSingleton<Eip712TypedDataSigner>();
        services.AddSingleton<EthereumMessageSigner>();
        services.AddSingleton<ISigner, Signer>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var rsa = RSA.Create(); // @@??: Important to not dispose?
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
                        if (context.Request.Path.StartsWithSegments("/hub"))
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

                        if (context.Request.Path.StartsWithSegments("/hub"))
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
        services.AddSingleton<ITotpProvider, TotpProvider>();

        services.AddSingleton<IFileReceiver, FileReceiver>();
        services.AddSingleton<MultipartRequestHelper>();
        services.AddSingleton<ImageFileValidator>();

        services.AddHttpClient("ipfs", (sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(configuration["IPFS:Host"]!);
        });
        services.AddSingleton<IFileStorage, FileStorage>();

        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IThingRepository, ThingRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IAcceptancePollVoteRepository, AcceptancePollVoteRepository>();
        services.AddScoped<ISettlementProposalRepository, SettlementProposalRepository>();
        services.AddScoped<IAssessmentPollVoteRepository, AssessmentPollVoteRepository>();
        services.AddScoped<IWatchedItemRepository, WatchedItemRepository>();
        services.AddScoped<IThingUpdateRepository, ThingUpdateRepository>();
        services.AddScoped<ISettlementProposalUpdateRepository, SettlementProposalUpdateRepository>();

        services.AddSingleton<IBlockProgressRepository, BlockProgressRepository>();
        services.AddScoped<IActionableThingRelatedEventRepository, ActionableThingRelatedEventRepository>();
        services.AddScoped<IPreJoinedThingSubmissionVerifierLotteryEventRepository, PreJoinedThingSubmissionVerifierLotteryEventRepository>();
        services.AddScoped<IJoinedThingSubmissionVerifierLotteryEventRepository, JoinedThingSubmissionVerifierLotteryEventRepository>();
        services.AddScoped<ICastedAcceptancePollVoteEventRepository, CastedAcceptancePollVoteEventRepository>();
        services.AddScoped<IPreJoinedThingAssessmentVerifierLotteryEventRepository, PreJoinedThingAssessmentVerifierLotteryEventRepository>();
        services.AddScoped<IJoinedThingAssessmentVerifierLotteryEventRepository, JoinedThingAssessmentVerifierLotteryEventRepository>();
        services.AddScoped<IThingAssessmentVerifierLotterySpotClaimedEventRepository, ThingAssessmentVerifierLotterySpotClaimedEventRepository>();
        services.AddScoped<ICastedAssessmentPollVoteEventRepository, CastedAssessmentPollVoteEventRepository>();

        services.AddScoped<ITagQueryable, TagQueryable>();
        services.AddScoped<ISubjectQueryable, SubjectQueryable>();
        services.AddScoped<IThingQueryable, ThingQueryable>();
        services.AddScoped<IThingSubmissionVerifierLotteryEventQueryable, ThingSubmissionVerifierLotteryEventQueryable>();
        services.AddScoped<ISettlementProposalAssessmentVerifierLotteryEventQueryable, SettlementProposalAssessmentVerifierLotteryEventQueryable>();
        services.AddScoped<IThingAcceptancePollVoteQueryable, ThingAcceptancePollVoteQueryable>();
        services.AddScoped<IThingAssessmentPollVoteQueryable, ThingAssessmentPollVoteQueryable>();
        services.AddScoped<ISettlementProposalQueryable, SettlementProposalQueryable>();
        services.AddScoped<IWatchListQueryable, WatchListQueryable>();

        services.AddSingleton<IContractEventListener, ContractEventListener>();

        services.AddSingleton<AccountProvider>();
        services.AddSingleton<IContractCaller, ContractCaller>();
        services.AddSingleton<IBlockListener, BlockListener>();
        services.AddSingleton<IBlockchainQueryable, BlockchainQueryable>();
        services.AddSingleton<IContractStorageQueryable, ContractStorageQueryable>();

        services.AddSingleton<IRequestDispatcher, RequestDispatcher>();

        if (!configuration.GetValue<bool>("DbMigrator"))
        {
            services.AddKafka(kafka =>
                kafka
                    .UseMicrosoftLog()
                    .AddCluster(cluster =>
                        cluster
                            .WithBrokers(configuration.GetSection("Kafka:Brokers").Get<List<string>>())
                            .AddConsumer(consumer =>
                                consumer
                                    .Topics(configuration.GetSection("Kafka:EventConsumer:Topics").Get<List<string>>())
                                    .WithGroupId(configuration["Kafka:EventConsumer:GroupId"])
                                    .WithAutoOffsetReset(AutoOffsetReset.Latest)
                                    .WithBufferSize(1)
                                    .WithWorkersCount(4)
                                    .AddMiddlewares(middlewares =>
                                        middlewares
                                            .AddSerializer<MessageSerializer, MessageTypeResolver>()
                                            .AddTypedHandlers(handlers =>
                                                handlers
                                                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                                                    .AddHandlers(new[]
                                                    {
                                                        typeof(ThingFundedEventHandler),
                                                        typeof(ThingSubmissionVerifierLotteryClosedInFailureEventHandler),
                                                        typeof(ThingSubmissionVerifierLotteryClosedWithSuccessEventHandler),
                                                        typeof(ThingAcceptancePollFinalizedEventHandler),
                                                        typeof(ThingSettlementProposalFundedEventHandler),
                                                        typeof(ThingSettlementProposalAssessmentVerifierLotteryClosedWithSuccessEventHandler),
                                                        typeof(ThingSettlementProposalAssessmentPollFinalizedEventHandler),
                                                        typeof(ThingUpdateEventHandler),
                                                        typeof(SettlementProposalUpdateEventHandler)
                                                    })
                                            )
                                    )
                            )
                            .AddConsumer(consumer =>
                                consumer
                                    .Topics(configuration.GetSection("Kafka:ResponseConsumer:Topics").Get<List<string>>())
                                    .WithGroupId(configuration["Kafka:ResponseConsumer:GroupId"])
                                    .WithAutoOffsetReset(AutoOffsetReset.Latest)
                                    .WithBufferSize(1)
                                    .WithWorkersCount(1)
                                    .AddMiddlewares(middlewares =>
                                        middlewares.Add<MessageConsumer>(MiddlewareLifetime.Scoped)
                                    )
                            )
                            .AddProducer<RequestDispatcher>(producer =>
                                producer
                                    .DefaultTopic(configuration["Kafka:RequestProducer:Topic"])
                                    .WithAcks(Acks.All)
                                    .AddMiddlewares(middlewares =>
                                        middlewares.AddSerializer<MessageSerializer, MessageTypeResolver>()
                                    )
                            )
                    )
            );
        }

        return services;
    }
}