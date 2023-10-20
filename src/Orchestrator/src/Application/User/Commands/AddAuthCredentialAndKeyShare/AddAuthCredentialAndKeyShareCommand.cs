using System.Security.Claims;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Fido2NetLib;
using MediatR;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Misc;

namespace Application.User.Commands.AddAuthCredentialAndKeyShare;

[ExecuteInTxn]
public class AddAuthCredentialAndKeyShareCommand : IRequest<HandleResult<AddAuthCredentialAndKeyShareResultVm>>
{
    public required AuthenticatorAttestationRawResponse RawAttestation { get; init; }
    public required string Nonce { get; init; }
    public required string SignatureOverNonce { get; init; }
    public required string KeyShare { get; init; }
}

internal class AddAuthCredentialAndKeyShareCommandHandler : IRequestHandler<
    AddAuthCredentialAndKeyShareCommand,
    HandleResult<AddAuthCredentialAndKeyShareResultVm>
>
{
    private readonly ILogger<AddAuthCredentialAndKeyShareCommandHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly ISigner _signer;
    private readonly ITotpProvider _totpProvider;
    private readonly IAuthTokenProvider _authTokenProvider;
    private readonly IContractCaller _contractCaller;

    public AddAuthCredentialAndKeyShareCommandHandler(
        ILogger<AddAuthCredentialAndKeyShareCommandHandler> logger,
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
        _userRepository = userRepository;
        _fido2 = fido2;
        _memoryCache = memoryCache;
        _signer = signer;
        _totpProvider = totpProvider;
        _authTokenProvider = authTokenProvider;
        _contractCaller = contractCaller;
    }

    public async Task<HandleResult<AddAuthCredentialAndKeyShareResultVm>> Handle(
        AddAuthCredentialAndKeyShareCommand command, CancellationToken ct
    )
    {
        var attestation = AuthenticatorAttestationResponse.Parse(command.RawAttestation);
        var optionsJson = _memoryCache.Get<string>($"fido2.attestationOptions.{Base64Url.Encode(attestation.Challenge)}");
        var options = CredentialCreateOptions.FromJson(optionsJson);

        IsCredentialIdUniqueToUserAsyncDelegate uniqueCheck = (args, ct) =>
            _userRepository.CheckCredentialIdUnique(Base64Url.Encode(args.CredentialId));

        // @@TODO: Handle exceptions.
        var result = await _fido2.MakeNewCredentialAsync(command.RawAttestation, options, uniqueCheck, ct);

        if (!_totpProvider.VerifyTotp(options.User.Id.ToHex(), command.Nonce)) // @@??: Use challenge instead of user id ?
        {
            _logger.LogWarning("Invalid nonce");
            return new()
            {
                Error = new UserError("Invalid nonce")
            };
        }

        var signerAddress = _signer.RecoverFromSiweMessage(command.Nonce, command.SignatureOverNonce);
        var walletAddress = await _contractCaller.GetWalletAddressFor(signerAddress);

        _logger.LogInformation("Signer: {Signer}\nWallet: {Wallet}", signerAddress, walletAddress);

        var userId = new Guid(options.User.Id).ToString();

        var user = await _userRepository.FindById(userId);
        var credential = new AuthCredential
        (
            id: Base64Url.Encode(result.Result!.Id),
            userId: userId,
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

        user!.AddAuthCredential(credential);

        var claims = new List<Claim>()
        {
            new("signer_address", signerAddress),
            new("wallet_address", walletAddress),
            new("key_share", command.KeyShare)
        };

        var error = await _userRepository.AddClaimsTo(user, claims);
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
                WalletAddress = walletAddress,
                Token = _authTokenProvider.GenerateJwt(userId, claims.SkipLast(1).ToList())
            }
        };
    }
}
