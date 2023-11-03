using Domain.Errors;

namespace Application.Common.Errors;

public class ServerError : HandleError
{
    public bool IsRetryable { get; }

    public ServerError(string message, bool isRetryable) : base("Server")
    {
        IsRetryable = isRetryable;
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}
