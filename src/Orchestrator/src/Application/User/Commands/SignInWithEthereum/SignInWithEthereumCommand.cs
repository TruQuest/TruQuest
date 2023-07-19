using System.Security.Claims;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using MediatR;
using FluentValidation;

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

internal class Validator : AbstractValidator<SignInWithEthereumCommand>
{
    public Validator()
    {
        RuleFor(c => c.Message).NotEmpty();
        RuleFor(c => c.Signature).NotEmpty();
    }
}

internal class SignInWithEthereumCommandHandler :
    IRequestHandler<SignInWithEthereumCommand, HandleResult<SignInWithEthereumResultVm>>
{
    private static readonly Regex _walletAddressRegex = new Regex(@"\s(0x[a-fA-F0-9]{40})\s");
    private static readonly Regex _nonceRegex = new Regex(@"\sNonce: (\d{6})\s");

    private readonly ILogger<SignInWithEthereumCommandHandler> _logger;
    private readonly ISigner _signer;
    private readonly ITotpProvider _totpProvider;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenProvider _authTokenProvider;
    private readonly IContractCaller _contractCaller;

    public SignInWithEthereumCommandHandler(
        ILogger<SignInWithEthereumCommandHandler> logger,
        ISigner signer,
        ITotpProvider totpProvider,
        IUserRepository userRepository,
        IAuthTokenProvider authTokenProvider,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _signer = signer;
        _totpProvider = totpProvider;
        _userRepository = userRepository;
        _authTokenProvider = authTokenProvider;
        _contractCaller = contractCaller;
    }

    public async Task<HandleResult<SignInWithEthereumResultVm>> Handle(
        SignInWithEthereumCommand command, CancellationToken ct
    )
    {
        var recoveredAddress = _signer.RecoverFromSiweMessage(command.Message, command.Signature);
        var walletAddress = _walletAddressRegex.Match(command.Message).Groups[1].Value;

        var walletAddressForOwner = await _contractCaller.GetWalletAddressFor(recoveredAddress);
        if (walletAddress.ToLower() != walletAddressForOwner.ToLower())
        {
            return new()
            {
                Error = new UserError("Not the owner of the wallet")
            };
        }

        _logger.LogInformation("Owner: {Owner}\nWallet: {Wallet}", recoveredAddress, walletAddress);

        var nonce = _nonceRegex.Match(command.Message).Groups[1].Value;
        if (!_totpProvider.VerifyTotp(walletAddress, nonce))
        {
            _logger.LogWarning("Invalid nonce");
            return new()
            {
                Error = new UserError("Invalid nonce")
            };
        }

        var userId = walletAddress.Substring(2).ToLower();
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
                UserName = walletAddress
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
