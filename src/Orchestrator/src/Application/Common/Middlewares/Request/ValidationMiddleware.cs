using GoThataway;
using FluentValidation;

using Domain.Results;
using Domain.Errors;

namespace Application.Common.Middlewares.Request;

public class ValidationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationMiddleware(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, ct))
            );
            var failures = validationResults
                .SelectMany(result => result.Errors)
                .ToList();

            if (failures.Any())
            {
                return new TResponse
                {
                    Error = new HandleError(string.Join(";\n", failures.Select(f => $"[{f.PropertyName}]: {f.ErrorMessage}")))
                };
            }
        }

        return await next();
    }
}
