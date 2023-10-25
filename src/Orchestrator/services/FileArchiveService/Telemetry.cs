using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

public static class Telemetry
{
    public const string ServiceName = "FileArchiveService";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);

    public static Activity? CurrentActivity => Activity.Current;

    public static void PropagateContextThrough<T>(
        ActivityContext context, T carrier, Action<T, string, string> setter
    )
    {
        Propagators.DefaultTextMapPropagator.Inject(
            new PropagationContext(context, Baggage.Current),
            carrier,
            setter
        );
    }

    public static ActivityContext ExtractContextFrom<T>(T carrier, Func<T, string, string?> getter)
    {
        var propagationContext = Propagators.DefaultTextMapPropagator.Extract(
            default,
            carrier,
            (carrier, key) =>
            {
                var value = getter(carrier, key);
                if (value != null) return new[] { value };
                return Enumerable.Empty<string>();
            });

        Baggage.Current = propagationContext.Baggage;

        return propagationContext.ActivityContext;
    }

    public static Activity? StartActivity(
        [CallerMemberName] string name = "", ActivityKind kind = ActivityKind.Internal
    ) => ActivitySource.StartActivity(name, kind);

    public static Activity? StartActivity(
        string name, ActivityKind kind, ActivityContext parentContext,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default
    ) => ActivitySource.StartActivity(name, kind, parentContext, tags, links, startTime);

    public static Activity? StartActivity(
        string name, ActivityKind kind, string? parentId,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default
    ) => ActivitySource.StartActivity(name, kind, parentId, tags, links, startTime);

    public static Activity? StartActivity(
        ActivityKind kind, ActivityContext parentContext = default,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default,
        [CallerMemberName] string name = ""
    ) => ActivitySource.StartActivity(kind, parentContext, tags, links, startTime, name);
}

public static class ActivityExtension
{
    public static Activity SetKafkaTags(this Activity span, string conversationId, string messageKey, string destinationName)
    {
        return span
            .SetTag("messaging.system", "kafka")
            .SetTag("messaging.operation", "publish")
            .SetTag("messaging.message.conversation_id", conversationId)
            .SetTag("messaging.destination.name", destinationName)
            .SetTag("messaging.kafka.message.key", messageKey);
    }
}
