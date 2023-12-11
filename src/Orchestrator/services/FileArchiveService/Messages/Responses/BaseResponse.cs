namespace Messages.Responses;

internal abstract class BaseResponse
{
    public virtual IEnumerable<(string Name, object? Value)> GetActivityTags() =>
        Enumerable.Empty<(string Name, object? Value)>();
}
