using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;

using Application.Common.Interfaces;

namespace Application.Account.Commands.SignUp;

public class SignUpCommand : IRequest<HandleResult<SignUpResultVM>>
{
    public SignUpIM Input { get; set; }
    public string Signature { get; set; }
}

public class SignUpCommandHandler : IRequestHandler<SignUpCommand, HandleResult<SignUpResultVM>>
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

    public async Task<HandleResult<SignUpResultVM>> Handle(SignUpCommand command, CancellationToken ct)
    {
        var result = _signer.RecoverFromSignUpMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new HandleResult<SignUpResultVM>
            {
                Error = result.Error
            };
        }

        var user = new User
        {
            Id = result.Data!.Replace("0x", string.Empty),
            UserName = command.Input.Username
        };

        var error = await _userRepository.Create(user);
        if (error != null)
        {
            return new HandleResult<SignUpResultVM>
            {
                Error = error
            };
        }

        await _userRepository.SaveChanges();

        var jwt = _authTokenProvider.GenerateJWT(user.Id);

        return new HandleResult<SignUpResultVM>
        {
            Data = new()
            {
                Token = jwt
            }
        };
    }
}