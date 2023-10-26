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
        string name,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMemberName = "",
        ActivityKind kind = ActivityKind.Internal
    )
    {
        var activityName = $"{Path.GetFileNameWithoutExtension(callerFilePath)}::{callerMemberName}::{name}";
        return ActivitySource.StartActivity(activityName, kind);
    }

    public static Activity? StartActivity(
        string name,
        ActivityContext parentContext,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMemberName = "",
        ActivityKind kind = ActivityKind.Internal
    )
    {
        var activityName = $"{Path.GetFileNameWithoutExtension(callerFilePath)}::{callerMemberName}::{name}";
        return ActivitySource.StartActivity(activityName, kind, parentContext);
    }

    public static Activity? StartActivity(
        string name,
        string? traceparent,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMemberName = "",
        ActivityKind kind = ActivityKind.Internal
    )
    {
        var activityName = $"{Path.GetFileNameWithoutExtension(callerFilePath)}::{callerMemberName}::{name}";
        return ActivitySource.StartActivity(activityName, kind, traceparent);
    }
}

public static class ActivityExtension
{
    public static string GetTraceparent(this Activity span)
    {
        // @@TODO: Construct traceparent manually.
        var carrier = new Dictionary<string, string>();
        Telemetry.PropagateContextThrough(span.Context, carrier, (carrier, key, value) =>
        {
            carrier[key] = value;
        });

        return carrier["traceparent"];
    }

    public static void AddTraceparentTo(this Activity span, Dictionary<string, object> carrier)
    {
        Telemetry.PropagateContextThrough(span.Context, carrier, (carrier, key, value) =>
        {
            carrier[key] = value;
        });
    }

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
