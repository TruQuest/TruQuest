using System.Diagnostics;
using System.Diagnostics.Metrics;

public static class Instrumentation
{
    public const string ServiceName = "FileArchiveService";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);
}