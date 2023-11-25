using System.Text.Json.Serialization;

namespace Domain.Errors;

public class UnhandledError : HandleError
{
    public bool IsUnhandled { get; } = true;
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public bool IsRetryable { get; }
    public string TraceId { get; }

    public UnhandledError(string message, string traceId, bool isRetryable) : base(message)
    {
        TraceId = traceId;
        IsRetryable = isRetryable;
    }
}
