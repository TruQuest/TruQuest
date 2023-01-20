using System.Security.Claims;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using UserDm = Domain.Aggregates.User;

using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.User.Commands.SignUp;

[ExecuteInTxn]
public class SignUpCommand : IRequest<HandleResult<SignUpResultVm>>
{
    public SignUpIm Input { get; set; }
    public string Signature { get; set; }
}

internal class SignUpCommandHandler : IRequestHandler<SignUpCommand, HandleResult<SignUpResultVm>>
{
    private readonly ILogger<SignUpCommandHandler> _logger;
    private readonly ISigner _signer;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenProvider _authTokenProvider;

    public SignUpCommandHandler(
        ILogger<SignUpCommandHandler> logger,
        ISigner signer,
        IUserRepository userRepository,
        IAuthTokenProvider authTokenProvider
    )
    {
        _logger = logger;
        _signer = signer;
        _userRepository = userRepository;
        _authTokenProvider = authTokenProvider;
    }

    public async Task<HandleResult<SignUpResultVm>> Handle(SignUpCommand command, CancellationToken ct)
    {
        var result = _signer.RecoverFromSignUpMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        var user = new UserDm
        {
            Id = result.Data!.ToLower(),
            UserName = command.Input.Username
        };

        var error = await _userRepository.Create(user);
        if (error != null)
        {
            return new()
            {
                Error = error
            };
        }

        var claims = new List<Claim>
        {
            new("username", user.UserName)
        };

        error = await _userRepository.AddClaimsTo(user, claims);
        if (error != null)
        {
            return new()
            {
                Error = error
            };
        }

        await _userRepository.SaveChanges();

        var jwt = _authTokenProvider.GenerateJwt(user.Id, claims);

        return new()
        {
            Data = new()
            {
                Token = jwt
            }
        };
    }
}