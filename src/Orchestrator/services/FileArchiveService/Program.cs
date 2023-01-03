using System.Net.Http.Headers;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

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

        services.AddSingleton<IImageSignatureVerifier, ImageSignatureVerifier>();
        services.AddSingleton<IImageSaver, ImageSaver>();
        services.AddSingleton<IWebPageSaver, WebPageSaver>();
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<IResponseDispatcher, ResponseDispatcher>();

        services.AddKafkaFlowHostedService(kafka =>
            kafka
                .UseMicrosoftLog()
                .AddCluster(cluster =>
                    cluster
                        .WithBrokers(configuration.GetSection("Kafka:Brokers").Get<List<string>>())
                        .AddConsumer(consumer =>
                            consumer
                                .Topic(configuration["Kafka:Consumer:Topic"])
                                .WithGroupId(configuration["Kafka:Consumer:GroupId"])
                                .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                                .WithBufferSize(1)
                                .WithWorkersCount(4)
                                .AddMiddlewares(middlewares =>
                                    middlewares
                                        .AddSerializer<MessageSerializer, MessageTypeResolver>()
                                        .AddTypedHandlers(handlers =>
                                            handlers
                                                .WithHandlerLifetime(InstanceLifetime.Singleton)
                                                .AddHandlersFromAssemblyOf<MessageTypeResolver>()
                                        )
                                )
                        )
                        .AddProducer<ResponseDispatcher>(producer =>
                            producer
                                .DefaultTopic(configuration["Kafka:Producer:Topic"])
                                .WithAcks(Acks.All)
                                .AddMiddlewares(middlewares =>
                                    middlewares.AddSerializer<MessageSerializer, MessageTypeResolver>()
                                )
                        )
                )
        );
    })
    .Build();

host.Run();
