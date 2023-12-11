namespace Messages.Requests;

internal abstract class BaseRequest
{
    public virtual IEnumerable<(string Name, object? Value)> GetActivityTags() =>
        Enumerable.Empty<(string Name, object? Value)>();
}
