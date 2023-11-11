using System.Text;
using System.Security.Claims;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Fido2NetLib;
using MediatR;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;
using UserDm = Domain.Aggregates.User;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.User.Common.Models.VM;

namespace Application.User.Commands.SignUp;

[ExecuteInTxn]
public class SignUpCommand : IRequest<HandleResult<AuthResultVm>>
{
    public required string Email { get; init; }
    public required string ConfirmationCode { get; init; }
    public required string SignatureOverCode { get; init; }
    public required AuthenticatorAttestationRawResponse RawAttestation { get; init; }
    public required string KeyShare { get; init; }
}

internal class SignUpCommandHandler : IRequestHandler<SignUpCommand, HandleResult<AuthResultVm>>
{
    private readonly ILogger<SignUpCommandHandler> _logger;
    private readonly IWhitelistQueryable _whitelistQueryable;
    private readonly IUserRepository _userRepository;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly ISigner _signer;
    private readonly ITotpProvider _totpProvider;
    private readonly IAuthTokenProvider _authTokenProvider;
    private readonly IContractCaller _contractCaller;

    public SignUpCommandHandler(
        ILogger<SignUpCommandHandler> logger,
        IWhitelistQueryable whitelistQueryable,
        IUserRepository userRepository,
        IFido2 fido2,
        IMemoryCache memoryCache,
        ISigner signer,
        ITotpProvider totpProvider,
        IAuthTokenProvider authTokenProvider,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _whitelistQueryable = whitelistQueryable;
        _userRepository = userRepository;
        _fido2 = fido2;
        _memoryCache = memoryCache;
        _signer = signer;
        _totpProvider = totpProvider;
        _authTokenProvider = authTokenProvider;
        _contractCaller = contractCaller;
    }

    public async Task<HandleResult<AuthResultVm>> Handle(SignUpCommand command, CancellationToken ct)
    {
        if (!await _whitelistQueryable.CheckIsWhitelisted(WhitelistEntryType.Email, command.Email))
        {
            return new()
            {
                Error = new AuthorizationError("Sorry, the access is currently restricted. Email me at admin@truquest.io to get access")
            };
        }

        if (!_totpProvider.VerifyTotp(Encoding.UTF8.GetBytes(command.Email), command.ConfirmationCode))
        {
            _logger.LogWarning("Invalid confirmation code");
            return new()
            {
                Error = new UserError("Invalid confirmation code")
            };
        }

        var attestation = AuthenticatorAttestationResponse.Parse(command.RawAttestation);

        var cacheKey = $"fido2.attestationOptions.{Base64Url.Encode(attestation.Challenge)}";
        if (!_memoryCache.TryGetValue<string>(cacheKey, out string? optionsJson))
        {
            return new()
            {
                Error = new UserError("Challenge expired")
            };
        }

        _memoryCache.Remove(cacheKey);
        var options = CredentialCreateOptions.FromJson(optionsJson);

        IsCredentialIdUniqueToUserAsyncDelegate checkCredentialIdUnique = (args, ct) =>
            _userRepository.CheckCredentialIdUnique(Base64Url.Encode(args.CredentialId));

        // @@TODO!!: Handle exceptions.
        var result = await _fido2.MakeNewCredentialAsync(command.RawAttestation, options, checkCredentialIdUnique, ct);

        var signerAddress = _signer.RecoverFromMessage(command.ConfirmationCode, command.SignatureOverCode);
        var walletAddress = await _contractCaller.GetWalletAddressFor(signerAddress);

        _logger.LogInformation("***** Signer: {Signer}\nWallet: {Wallet} *****", signerAddress, walletAddress);

        var user = new UserDm
        {
            Id = new Guid(options.User.Id).ToString(),
            Email = command.Email,
            UserName = signerAddress,
            WalletAddress = walletAddress,
            EmailConfirmed = true
        };

        var credential = new AuthCredential
        (
            id: Base64Url.Encode(result.Result!.Id),
            userId: user.Id,
            publicKey: Base64Url.Encode(result.Result.PublicKey),
            signCount: (int)result.Result.SignCount,
            isBackupEligible: result.Result.IsBackupEligible,
            isBackedUp: result.Result.IsBackedUp,
            attestationObject: Base64Url.Encode(result.Result.AttestationObject),
            attestationClientDataJSON: Base64Url.Encode(result.Result.AttestationClientDataJson),
            attestationFormat: result.Result.AttestationFormat,
            addedAt: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            aaGuid: result.Result.AaGuid
        );
        credential.AddTransports(result.Result.Transports?.Select(t => (int)t).ToList());

        user.AddAuthCredential(credential);

        var error = await _userRepository.Create(user);
        if (error != null)
        {
            return new()
            {
                Error = error
            };
        }

        var claims = new List<Claim>()
        {
            new("signer_address", signerAddress),
            new("wallet_address", walletAddress),
            new("key_share", command.KeyShare)
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

        return new()
        {
            Data = new()
            {
                UserId = user.Id,
                SignerAddress = signerAddress,
                WalletAddress = walletAddress,
                Token = _authTokenProvider.GenerateJwt(user.Id, claims.SkipLast(1).ToList())
            }
        };
    }
}
