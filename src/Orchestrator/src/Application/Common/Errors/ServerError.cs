using Domain.Errors;

namespace Application.Common.Errors;

public class ServerError : HandleError
{
    public ServerError(string message) : base("Server")
    {
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}