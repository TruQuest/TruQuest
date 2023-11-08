namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TrackGasUsageAttribute : Attribute
{
    public required string MetricName { get; init; }
}
