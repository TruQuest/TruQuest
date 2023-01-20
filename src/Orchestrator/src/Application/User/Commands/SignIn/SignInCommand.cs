using MediatR;

using Domain.Results;
using Domain.Errors;
using Domain.Aggregates;

using Application.Common.Interfaces;

namespace Application.User.Commands.SignIn;

public class SignInCommand : IRequest<HandleResult<SignInResultVm>>
{
    public required string Timestamp { get; init; }
    public required string OrchestratorSignature { get; init; }
    public required string Signature { get; init; }
}

internal class SignInCommandHandler : IRequestHandler<SignInCommand, HandleResult<SignInResultVm>>
{
    private readonly ISigner _signer;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenProvider _authTokenProvider;

    public SignInCommandHandler(
        ISigner signer,
        IUserRepository userRepository,
        IAuthTokenProvider authTokenProvider
    )
    {
        _signer = signer;
        _userRepository = userRepository;
        _authTokenProvider = authTokenProvider;
    }

    public async Task<HandleResult<SignInResultVm>> Handle(SignInCommand command, CancellationToken ct)
    {
        if (!_signer.CheckOrchestratorSignatureOnTimestamp(command.Timestamp, command.OrchestratorSignature))
        {
            return new()
            {
                Error = new UserError("Invalid sign-in data")
            };
        }

        var signInDataIssuedAt = DateTimeOffset.Parse(command.Timestamp);
        if ((DateTimeOffset.UtcNow - signInDataIssuedAt) > TimeSpan.FromMinutes(5))
        {
            return new()
            {
                Error = new UserError("Sign-in request expired")
            };
        }

        var address = _signer.RecoverFromSignInMessage(
            command.Timestamp, command.OrchestratorSignature, command.Signature
        );

        var userId = address.ToLower();

        var result = await _userRepository.GetClaimsFor(userId);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        var claims = result.Data!;

        return new()
        {
            Data = new()
            {
                Username = claims.Single(c => c.Type == "username").Value,
                Token = _authTokenProvider.GenerateJwt(userId, claims)
            }
        };
    }
}