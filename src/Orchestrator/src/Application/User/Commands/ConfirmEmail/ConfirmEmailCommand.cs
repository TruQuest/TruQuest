using System.Diagnostics;

using Microsoft.Extensions.Logging;

using MediatR;
using FluentValidation;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.User.Commands.ConfirmEmail;

[RequireAuthorization]
public class ConfirmEmailCommand : IRequest<VoidResult>
{
    public required string ConfirmationToken { get; init; }
}

internal class Validator : AbstractValidator<ConfirmEmailCommand>
{
    public Validator()
    {
        RuleFor(c => c.ConfirmationToken).NotEmpty();
    }
}

internal class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, VoidResult>
{
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IUserRepository _userRepository;
    private readonly IConfirmationTokenProvider _confirmationTokenProvider;

    public ConfirmEmailCommandHandler(
        ILogger<ConfirmEmailCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        IUserRepository userRepository,
        IConfirmationTokenProvider confirmationTokenProvider
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _userRepository = userRepository;
        _confirmationTokenProvider = confirmationTokenProvider;
    }

    public async Task<VoidResult> Handle(ConfirmEmailCommand command, CancellationToken ct)
    {
        var user = await _userRepository.FindById(_currentPrincipal.Id!);
        Debug.Assert(user != null);

        if (
            await _confirmationTokenProvider.VerifyEmailConfirmationToken(user, command.ConfirmationToken)
        )
        {
            user.EmailConfirmed = true;
            await _userRepository.SaveChanges();
        }
        else
        {
            _logger.LogInformation("User {UserId}: Invalid email confirmation token", user.Id);
        }

        return VoidResult.Instance;
    }
}
