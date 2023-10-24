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
using Application.User.Common.Models.VM;

namespace Application.User.Commands.SignInWithEthereum;

[ExecuteInTxn]
public class SignInWithEthereumCommand : IRequest<HandleResult<AuthResultVm>>
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
    IRequestHandler<SignInWithEthereumCommand, HandleResult<AuthResultVm>>
{
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

    public async Task<HandleResult<AuthResultVm>> Handle(SignInWithEthereumCommand command, CancellationToken ct)
    {
        var signerAddress = _signer.RecoverFromSiweMessage(command.Message, command.Signature);

        var nonce = _nonceRegex.Match(command.Message).Groups[1].Value;
        if (!_totpProvider.VerifyTotp(signerAddress, nonce))
        {
            _logger.LogWarning("Invalid nonce");
            return new()
            {
                Error = new UserError("Invalid nonce")
            };
        }

        var walletAddress = await _contractCaller.GetWalletAddressFor(signerAddress);
        _logger.LogInformation("***** Signer: {Signer}. Wallet: {Wallet} *****", signerAddress, walletAddress);

        IList<Claim> claims;
        var user = await _userRepository.FindByUsername(signerAddress);
        if (user != null)
        {
            claims = await _userRepository.GetClaimsExcept(user.Id, new[] { "key_share" });
        }
        else
        {
            user = new UserDm
            {
                Id = Guid.NewGuid().ToString(),
                UserName = signerAddress
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
                new("signer_address", signerAddress),
                new("wallet_address", walletAddress)
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

        return new()
        {
            Data = new()
            {
                UserId = user.Id,
                SignerAddress = signerAddress,
                WalletAddress = walletAddress,
                Token = _authTokenProvider.GenerateJwt(user.Id, claims)
            }
        };
    }
}
