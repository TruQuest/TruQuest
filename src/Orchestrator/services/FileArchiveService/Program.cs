using System.Net.Http.Headers;

using KafkaFlow;
using KafkaFlow.TypedHandler;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Services;

Action<ResourceBuilder> configureResource = resource =>
    resource.AddService(
        serviceName: Telemetry.ServiceName,
        serviceVersion: "0.1.0",
        serviceInstanceId: Environment.MachineName
    );

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((hostContext, loggingBuilder) =>
    {
        var configuration = hostContext.Configuration;

        loggingBuilder.ClearProviders();
        loggingBuilder.AddDebug();
        loggingBuilder.AddConsole();

        loggingBuilder.AddOpenTelemetry(options =>
        {
            var resourceBuilder = ResourceBuilder.CreateDefault();
            configureResource(resourceBuilder);
            options.SetResourceBuilder(resourceBuilder);

            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(configuration["Seq:Endpoint"]!);
                otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        services.AddOpenTelemetry()
            .ConfigureResource(configureResource)
            .WithTracing(builder =>
                builder
                    .AddSource(Telemetry.ActivitySource.Name)
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]!);
                    })
            )
            .WithMetrics(builder =>
                builder
                    .AddMeter(Telemetry.Meter.Name)
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]!);
                    })
            );

        var mimeTypes = configuration["Images:AcceptableMimeTypes"]!.Split(',');
        services.AddHttpClient("image", (sp, client) =>
        {
            foreach (var mimeType in mimeTypes)
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mimeType));
            }
        });

        services.AddHttpClient("ipfs", (sp, client) =>
        {
            client.BaseAddress = new Uri(configuration["IPFS:Host"]!);
        });

        services.AddSingleton<IImageSignatureVerifier, ImageSignatureVerifier>();
        services.AddSingleton<IImageSaver, ImageSaver>();
        services.AddSingleton<IImageCropper, ImageCropper>();
        if (configuration.GetValue<string>("WebPageScreenshots:Backend") == "Playwright")
        {
            services.AddSingleton<IWebPageScreenshotTaker, WebPageScreenshotTakerUsingPlaywright>();
        }
        else
        {
            services.AddSingleton<IWebPageScreenshotTaker, WebPageScreenshotTakerUsingApiFlash>();
        }
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<IFileArchiver, FileArchiver>();
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
                                        .Add<TelemetryMiddleware>(MiddlewareLifetime.Singleton)
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
