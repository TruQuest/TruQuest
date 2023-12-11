namespace Application.Common.Messages.Requests;

public abstract class BaseRequest
{
    public virtual IEnumerable<(string Name, object? Value)> GetActivityTags() =>
        Enumerable.Empty<(string Name, object? Value)>();
}
