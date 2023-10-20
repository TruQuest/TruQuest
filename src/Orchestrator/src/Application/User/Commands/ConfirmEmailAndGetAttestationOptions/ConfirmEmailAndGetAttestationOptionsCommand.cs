using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Fido2NetLib;
using Fido2NetLib.Objects;
using MediatR;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Misc;

namespace Application.User.Commands.ConfirmEmailAndGetAttestationOptions;

public class ConfirmEmailAndGetAttestationOptionsCommand : IRequest<HandleResult<ConfirmEmailAndGetAttestationOptionsResultVm>>
{
    public required string Email { get; init; }
    public required string ConfirmationCode { get; init; }
}

internal class ConfirmEmailAndGetAttestationOptionsCommandHandler : IRequestHandler<
    ConfirmEmailAndGetAttestationOptionsCommand,
    HandleResult<ConfirmEmailAndGetAttestationOptionsResultVm>
>
{
    private readonly ILogger<ConfirmEmailAndGetAttestationOptionsCommandHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly ITotpProvider _totpProvider;

    public ConfirmEmailAndGetAttestationOptionsCommandHandler(
        ILogger<ConfirmEmailAndGetAttestationOptionsCommandHandler> logger,
        IUserRepository userRepository,
        IFido2 fido2,
        IMemoryCache memoryCache,
        ITotpProvider totpProvider
    )
    {
        _logger = logger;
        _userRepository = userRepository;
        _fido2 = fido2;
        _memoryCache = memoryCache;
        _totpProvider = totpProvider;
    }

    public async Task<HandleResult<ConfirmEmailAndGetAttestationOptionsResultVm>> Handle(
        ConfirmEmailAndGetAttestationOptionsCommand command,
        CancellationToken ct
    )
    {
        var user = await _userRepository.FindByEmail(command.Email);
        user!.EmailConfirmed = true;
        await _userRepository.SaveChanges();

        var userIdBytes = Guid.Parse(user.Id).ToByteArray();
        var fido2User = new Fido2User
        {
            Id = userIdBytes,
            Name = user.Email,
            DisplayName = user.Email
        };

        var existingCredentials = (await _userRepository.GetAuthCredentialDescriptorsFor(user.Id))
            .Select(c => new PublicKeyCredentialDescriptor(
                PublicKeyCredentialType.PublicKey,
                Base64Url.Decode(c.Id),
                c.Transports?.Select(t => (AuthenticatorTransport)t).ToArray()
            ))
            .ToList();

        var authenticatorSelection = new AuthenticatorSelection
        {
            AuthenticatorAttachment = AuthenticatorAttachment.Platform,
            ResidentKey = ResidentKeyRequirement.Required,
            UserVerification = UserVerificationRequirement.Discouraged
        };

        // var exts = new AuthenticationExtensionsClientInputs()
        // {
        //     Extensions = true,
        //     PRF = new()
        //     {
        //         Eval = new()
        //         {
        //             First = Enumerable.Repeat<byte>(5, 32).ToArray()
        //         }
        //     }
        // };

        var options = _fido2.RequestNewCredential(
            fido2User,
            existingCredentials,
            authenticatorSelection,
            AttestationConveyancePreference.None
        );

        var challenge = Base64Url.Encode(options.Challenge);
        _logger.LogInformation($"******** Attestation Challenge: {challenge} ********");

        _memoryCache.Set($"fido2.attestationOptions.{challenge}", options.ToJson()); // @@TODO: Expiration.

        return new()
        {
            Data = new()
            {
                Options = options,
                Nonce = _totpProvider.GenerateTotpFor(userIdBytes.ToHex())
            }
        };
    }
}
