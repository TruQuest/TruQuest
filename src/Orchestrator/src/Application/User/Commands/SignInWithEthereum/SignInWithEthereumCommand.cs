using System.Security.Claims;
using System.Text.RegularExpressions;

using MediatR;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;
using UserDm = Domain.Aggregates.User;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.User.Commands.SignInWithEthereum;

[ExecuteInTxn]
public class SignInWithEthereumCommand : IRequest<HandleResult<SignInWithEthereumResultVm>>
{
    public required string Message { get; init; }
    public required string Signature { get; init; }
}

internal class SignInWithEthereumCommandHandler :
    IRequestHandler<SignInWithEthereumCommand, HandleResult<SignInWithEthereumResultVm>>
{
    private static readonly Regex _regex = new Regex(@"\sNonce: (\d{6})\s");

    private readonly ISigner _signer;
    private readonly ITotpProvider _totpProvider;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenProvider _authTokenProvider;

    public SignInWithEthereumCommandHandler(
        ISigner signer,
        ITotpProvider totpProvider,
        IUserRepository userRepository,
        IAuthTokenProvider authTokenProvider
    )
    {
        _signer = signer;
        _totpProvider = totpProvider;
        _userRepository = userRepository;
        _authTokenProvider = authTokenProvider;
    }

    public async Task<HandleResult<SignInWithEthereumResultVm>> Handle(
        SignInWithEthereumCommand command, CancellationToken ct
    )
    {
        var address = _signer.RecoverFromSiweMessage(command.Message, command.Signature);
        var nonce = _regex.Match(command.Message).Groups[1].Value;
        if (!_totpProvider.VerifyTotp(address, nonce))
        {
            return new()
            {
                Error = new UserError("Invalid nonce")
            };
        }

        var userId = address.ToLower();
        IList<Claim> claims;

        var user = await _userRepository.FindById(userId);
        if (user != null)
        {
            claims = await _userRepository.GetClaimsFor(user);
        }
        else
        {
            user = new UserDm
            {
                Id = userId,
                UserName = "0x" + address
            };

            var error = await _userRepository.Create(user);
            if (error != null)
            {
                return new()
                {
                    Error = error
                };
            }

            claims = new List<Claim>()
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
        }

        var jwt = _authTokenProvider.GenerateJwt(user.Id, claims);

        return new()
        {
            Data = new()
            {
                Username = user.UserName!,
                Token = jwt
            }
        };
    }
}