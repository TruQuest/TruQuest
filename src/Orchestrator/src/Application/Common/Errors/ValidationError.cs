using Domain.Errors;

namespace Application.Common.Errors;

public class ValidationError : HandleError
{
    public ValidationError(string message) : base("Validation")
    {
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}