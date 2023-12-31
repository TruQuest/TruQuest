using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;
using Dapper;
using KafkaFlow;
using KafkaFlow.Configuration;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.BlockchainProcessing.ProgressRepositories;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.Extensions.NETCore.Setup;

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
using Infrastructure.Persistence.Queryables;
using Infrastructure.Ethereum.ERC4337;
using Infrastructure.Email;

namespace Infrastructure;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration
    )
    {
        DbConnectionProvider.ConnectionString = configuration.GetConnectionString("Postgres")!;

#pragma warning disable CS0618
        NpgsqlConnection.GlobalTypeMapper.MapEnum<SubjectType>("truquest.subject_type");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ThingState>("truquest.thing_state");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<SettlementProposalState>("truquest.settlement_proposal_state");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Verdict>("truquest.verdict");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<TaskType>("truquest.task_type");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<WatchedItemType>("truquest.watched_item_type");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<WhitelistEntryType>("truquest.whitelist_entry_type");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<DeadLetterSource>("truquest.dead_letter_source");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<DeadLetterState>("truquest.dead_letter_state");
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ThingEventType>("truquest_events.thing_event_type");
#pragma warning restore CS0618

        services.AddDbContext<AppDbContext>(optionsBuilder =>
            optionsBuilder
                .UseNpgsql(
                    configuration.GetConnectionString("Postgres") + "SearchPath=truquest;",
                    pgOptionsBuilder => pgOptionsBuilder.MigrationsHistoryTable(
                        "__EFMigrationsHistory", "truquest"
                    )
                )
                .EnableSensitiveDataLogging(environment.EnvironmentName is "Development" or "Testing")
        );

        services.AddDbContext<EventDbContext>(optionsBuilder =>
            optionsBuilder
                .UseNpgsql(
                    configuration.GetConnectionString("Postgres") + "SearchPath=truquest_events;",
                    pgOptionsBuilder => pgOptionsBuilder.MigrationsHistoryTable(
                        "__EFMigrationsHistory", "truquest_events"
                    )
                )
                .EnableSensitiveDataLogging(environment.EnvironmentName is "Development" or "Testing")
        );

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services
            .AddIdentityCore<UserDm>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                options.User.AllowedUserNameCharacters = "0x123456789aAbBcCdDeEfF";
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddDataProtection();
        services.AddHttpContextAccessor();

        services.AddMemoryCache();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IAuthTokenProvider, AuthTokenProvider>();
        services.AddSingleton<IKeccakHasher, KeccakHasher>();
        services.AddSingleton<Eip712TypedDataSigner>();
        services.AddSingleton<EthereumMessageSigner>();
        services.AddSingleton<ISigner, Signer>();
        if (environment.EnvironmentName is "Development" or "Testing")
        {
            services.AddSingleton<IEmailSender, DummyEmailSender>();
            services.AddSingleton<IEmailForwarder, DummyEmailForwarder>();
        }
        else
        {
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IEmailForwarder, EmailForwarder>();
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var rsa = RSA.Create(); // @@??: Important to not dispose?
                rsa.FromXmlString(configuration["JWT:PublicKey"]!);

                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = environment.EnvironmentName is not ("Development" or "Testing");
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
                        if (context.Request.Path.StartsWithSegments("/api/hub"))
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                                context.Token = accessToken;
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

                        if (context.Request.Path.StartsWithSegments("/api/hub"))
                            authenticationContext.Token = context.SecurityToken;

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

        services.AddAuthorizationCore(options =>
            options.AddPolicy("AdminOnly", builder => builder.RequireClaim("is_admin"))
        );
        services.AddScoped<IAuthenticationContext, AuthenticationContext>();
        services.AddTransient<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ICurrentPrincipal, CurrentPrincipal>();
        services.AddSingleton<ITotpProvider, TotpProvider>();

        services.AddSingleton<IFileReceiver, FileReceiver>();
        services.AddSingleton<MultipartRequestHelper>();
        services.AddSingleton<ImageFileValidator>();

        services.AddHttpClient("ipfs", (sp, client) =>
        {
            client.BaseAddress = new Uri(configuration["IPFS:Host"]!);
        });
        services.AddSingleton<IFileStorage, FileStorage>();

        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IThingRepository, ThingRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IThingValidationPollVoteRepository, ThingValidationPollVoteRepository>();
        services.AddScoped<ISettlementProposalRepository, SettlementProposalRepository>();
        services.AddScoped<ISettlementProposalAssessmentPollVoteRepository, SettlementProposalAssessmentPollVoteRepository>();
        services.AddScoped<IWatchedItemRepository, WatchedItemRepository>();
        services.AddScoped<IThingUpdateRepository, ThingUpdateRepository>();
        services.AddScoped<ISettlementProposalUpdateRepository, SettlementProposalUpdateRepository>();
        services.AddScoped<IWhitelistRepository, WhitelistRepository>();
        services.AddScoped<IDeadLetterRepository, DeadLetterRepository>();

        services.AddSingleton<IBlockProgressRepository, BlockProgressRepository>();
        services.AddScoped<IActionableThingRelatedEventRepository, ActionableThingRelatedEventRepository>();
        services.AddScoped<IThingValidationVerifierLotteryInitializedEventRepository, ThingValidationVerifierLotteryInitializedEventRepository>();
        services.AddScoped<IJoinedThingValidationVerifierLotteryEventRepository, JoinedThingValidationVerifierLotteryEventRepository>();
        services.AddScoped<ICastedThingValidationPollVoteEventRepository, CastedThingValidationPollVoteEventRepository>();
        services.AddScoped<ISettlementProposalAssessmentVerifierLotteryInitializedEventRepository, SettlementProposalAssessmentVerifierLotteryInitializedEventRepository>();
        services.AddScoped<IJoinedSettlementProposalAssessmentVerifierLotteryEventRepository, JoinedSettlementProposalAssessmentVerifierLotteryEventRepository>();
        services.AddScoped<IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository, ClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository>();
        services.AddScoped<ICastedSettlementProposalAssessmentPollVoteEventRepository, CastedSettlementProposalAssessmentPollVoteEventRepository>();

        SqlMapper.AddTypeHandler(new DictionaryStringToStringTypeHandler());
        SqlMapper.AddTypeHandler(new DictionaryStringToObjectTypeHandler());

        services.AddScoped<IBlockProgressQueryable, BlockProgressQueryable>();
        services.AddScoped<ITagQueryable, TagQueryable>();
        services.AddScoped<ISubjectQueryable, SubjectQueryable>();
        services.AddScoped<IThingQueryable, ThingQueryable>();
        services.AddScoped<IThingValidationVerifierLotteryEventQueryable, ThingValidationVerifierLotteryEventQueryable>();
        services.AddScoped<ISettlementProposalAssessmentVerifierLotteryEventQueryable, SettlementProposalAssessmentVerifierLotteryEventQueryable>();
        services.AddScoped<IThingValidationPollVoteQueryable, ThingValidationPollVoteQueryable>();
        services.AddScoped<ISettlementProposalAssessmentPollVoteQueryable, SettlementProposalAssessmentPollVoteQueryable>();
        services.AddScoped<ISettlementProposalQueryable, SettlementProposalQueryable>();
        services.AddScoped<IWatchListQueryable, WatchListQueryable>();
        services.AddScoped<ITaskQueryable, TaskQueryable>();
        services.AddScoped<IWhitelistQueryable, WhitelistQueryable>();
        services.AddScoped<IDeadLetterQueryable, DeadLetterQueryable>();

        services.AddSingleton<IContractEventListener, ContractEventListener>();

        if (environment.EnvironmentName is "Development" or "Testing")
            services.AddSingleton<IAccountProvider, AccountProviderDev>();
        else
            services.AddSingleton<IAccountProvider, AccountProvider>();

        services.AddSingleton<IContractCaller, ContractCaller>();

        var network = configuration["Ethereum:Network"]!;
        if (network == "Ganache")
            services.AddSingleton<IBlockListener, BlockListener>();
        else if (network == "OPL2")
            services.AddSingleton<IBlockListener, OPL1BlockListener>();

        services.AddSingleton<IL1BlockchainQueryable, L1BlockchainQueryable>();
        services.AddSingleton<IL2BlockchainQueryable, L2BlockchainQueryable>();
        services.AddSingleton<IEthereumAddressFormatter, EthereumAddressFormatter>();

        if (environment.EnvironmentName is "Development" or "Testing")
        {
            services.AddSingleton<AbiEncoder>();
            services.AddSingleton<BundlerApi>();
            services.AddTransient<UserOperationBuilder>();
            services.AddSingleton<UserOperationService>();

            services.AddHttpClient("bundler", (sp, client) =>
            {
                client.BaseAddress = new Uri(configuration[$"Ethereum:Bundler:{network}:URL"]!);
            });
        }

        services.AddSingleton<IRequestDispatcher, RequestDispatcher>();

        services.AddKafka(kafka =>
            kafka
                .UseMicrosoftLog()
                .AddCluster(cluster =>
                    cluster
                        .WithBrokers(configuration.GetSection("Kafka:Brokers").Get<List<string>>())
                        .WithAutoCreateTopics(
                            configuration.GetSection("Kafka:EventConsumer:Topics").Get<List<string>>()!
                                .Concat(configuration.GetSection("Kafka:ResponseConsumer:Topics").Get<List<string>>()!)
                                .Concat(new[] { configuration["Kafka:RequestProducer:Topic"]! })
                                .Select(topic => (Name: topic, Partitions: 1, ReplicationFactor: (short)1))
                        )
                        .AddConsumer(consumer =>
                            consumer
                                .Topics(configuration.GetSection("Kafka:EventConsumer:Topics").Get<List<string>>())
                                .WithGroupId(configuration["Kafka:EventConsumer:GroupId"])
                                // @@!!: Fsr setting this to 'Latest' results in the first new message being missed. Why?
                                .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                                .WithBufferSize(1)
                                .WithWorkersCount(1)
                                .AddMiddlewares(middlewares =>
                                    middlewares
                                        .AddDeserializer<MessageSerializer, EventTypeResolver>()
                                        .Add<EventTracingMiddleware>(MiddlewareLifetime.Singleton)
                                        .Add<RetryOrArchiveMiddleware>(MiddlewareLifetime.Singleton)
                                        .Add<EventConsumer>(MiddlewareLifetime.Singleton)
                                )
                        )
                        .AddConsumer(consumer =>
                            consumer
                                .Topics(configuration.GetSection("Kafka:ResponseConsumer:Topics").Get<List<string>>())
                                .WithGroupId(configuration["Kafka:ResponseConsumer:GroupId"])
                                .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                                .WithBufferSize(1)
                                .WithWorkersCount(1)
                                .AddMiddlewares(middlewares =>
                                    middlewares
                                        .AddDeserializer<MessageSerializer, MessageTypeResolver>()
                                        .Add<ResponseTracingMiddleware>(MiddlewareLifetime.Singleton)
                                        .Add<MessageConsumer>(MiddlewareLifetime.Message)
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

        if (environment.EnvironmentName is "Development" or "Testing")
        {
            var awsOptions = new AWSOptions
            {
                Credentials = new BasicAWSCredentials(configuration["AWS_ACCESS_KEY_ID"], configuration["AWS_SECRET_ACCESS_KEY"])
            };
            services.AddDefaultAWSOptions(awsOptions);
        }
        services.AddAWSService<IAmazonS3>();

        return services;
    }
}

public static class IClusterConfigurationBuilderExtension
{
    public static IClusterConfigurationBuilder WithAutoCreateTopics(
        this IClusterConfigurationBuilder builder,
        IEnumerable<(string Name, int Partitions, short ReplicationFactor)> topics
    )
    {
        foreach (var topic in topics)
            builder.CreateTopicIfNotExists(topic.Name, topic.Partitions, topic.ReplicationFactor);

        return builder;
    }
}
