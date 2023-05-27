using FluentValidation.Results;

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

    internal ValidationError(IEnumerable<ValidationFailure> failures) : base(type: "Validation")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}