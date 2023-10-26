namespace Application.Common.Models.IM;

public abstract class DeferredTaskCommand
{
    public required string Traceparent { get; init; }
}
