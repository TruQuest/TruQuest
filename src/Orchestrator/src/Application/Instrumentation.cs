using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Application;

public static class Instrumentation
{
    public const string ServiceName = "Orchestrator";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);

    public static readonly Counter<int> TestCounter = Meter.CreateCounter<int>("test-counter");
}