using System.Diagnostics;

using MediatR;
using FluentValidation;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.User.Commands.AddEmail;

[RequireAuthorization]
public class AddEmailCommand : IRequest<VoidResult>
{
    public required string Email { get; init; }
}

internal class Validator : AbstractValidator<AddEmailCommand>
{
    public Validator()
    {
        RuleFor(c => c.Email).EmailAddress();
    }
}

internal class AddEmailCommandHandler : IRequestHandler<AddEmailCommand, VoidResult>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IUserRepository _userRepository;
    private readonly IConfirmationTokenProvider _confirmationTokenProvider;
    private readonly IEmailSender _emailSender;

    public AddEmailCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IUserRepository userRepository,
        IConfirmationTokenProvider confirmationTokenProvider,
        IEmailSender emailSender
    )
    {
        _currentPrincipal = currentPrincipal;
        _userRepository = userRepository;
        _confirmationTokenProvider = confirmationTokenProvider;
        _emailSender = emailSender;
    }

    public async Task<VoidResult> Handle(AddEmailCommand command, CancellationToken ct)
    {
        var user = await _userRepository.FindById(_currentPrincipal.Id!);
        Debug.Assert(user != null);

        var token = await _confirmationTokenProvider.GetEmailConfirmationToken(user);

        user.Email = command.Email;
        await _userRepository.SaveChanges();

        await _emailSender.Send(command.Email, $"Confirmation token: {token}");

        return VoidResult.Instance;
    }
}
