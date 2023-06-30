using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace Application;

public static class Telemetry
{
    public const string ServiceName = "Orchestrator";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);

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