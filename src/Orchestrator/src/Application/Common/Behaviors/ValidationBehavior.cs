using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Errors;

namespace Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
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
                    Error = new ValidationError(failures)
                };
            }
        }

        return await next();
    }
}