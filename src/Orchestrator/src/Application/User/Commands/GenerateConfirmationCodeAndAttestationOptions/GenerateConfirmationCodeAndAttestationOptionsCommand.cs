using System.Text;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using GoThataway;
using Fido2NetLib;
using Fido2NetLib.Objects;
using FluentValidation;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Interfaces;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.User.Commands.GenerateConfirmationCodeAndAttestationOptions;

public class GenerateConfirmationCodeAndAttestationOptionsCommand : IRequest<HandleResult<CredentialCreateOptions>>
{
    public required string Email { get; init; }
}

internal class Validator : AbstractValidator<GenerateConfirmationCodeAndAttestationOptionsCommand>
{
    public Validator()
    {
        RuleFor(c => c.Email).EmailAddress();
    }
}

public class GenerateConfirmationCodeAndAttestationOptionsCommandHandler : IRequestHandler<
    GenerateConfirmationCodeAndAttestationOptionsCommand,
    HandleResult<CredentialCreateOptions>
>
{
    private readonly ILogger<GenerateConfirmationCodeAndAttestationOptionsCommandHandler> _logger;
    private readonly IWhitelistQueryable _whitelistQueryable;
    private readonly IUserRepository _userRepository;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly ITotpProvider _totpProvider;
    private readonly IEmailSender _emailSender;

    public GenerateConfirmationCodeAndAttestationOptionsCommandHandler(
        ILogger<GenerateConfirmationCodeAndAttestationOptionsCommandHandler> logger,
        IWhitelistQueryable whitelistQueryable,
        IUserRepository userRepository,
        IFido2 fido2,
        IMemoryCache memoryCache,
        ITotpProvider totpProvider,
        IEmailSender emailSender
    )
    {
        _logger = logger;
        _whitelistQueryable = whitelistQueryable;
        _userRepository = userRepository;
        _fido2 = fido2;
        _memoryCache = memoryCache;
        _totpProvider = totpProvider;
        _emailSender = emailSender;
    }

    public async Task<HandleResult<CredentialCreateOptions>> Handle(GenerateConfirmationCodeAndAttestationOptionsCommand command, CancellationToken ct)
    {
        if (!await _whitelistQueryable.CheckIsWhitelisted(WhitelistEntryType.Email, command.Email))
        {
            _logger.LogInformation($"User with non-whitelisted email {Email} attempted to initiate sign-up process", command.Email);
            return new()
            {
                Error = new HandleError("Sorry, the access is currently restricted. Email me at admin@truquest.io to get access")
            };
        }

        var user = await _userRepository.FindByEmail(command.Email); // @@TODO: Use queryable instead.
        if (user != null)
        {
            _logger.LogInformation($"User with email {Email} already exists", command.Email);
            return new()
            {
                Error = new HandleError($"User with email {command.Email} already exists")
            };
        }

        var fido2User = new Fido2User
        {
            Id = Guid.NewGuid().ToByteArray(),
            Name = command.Email,
            DisplayName = command.Email
        };

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

        // @@TODO??: Can this fail?
        var options = _fido2.RequestNewCredential(
            fido2User,
            new(),
            authenticatorSelection,
            AttestationConveyancePreference.None
        );

        var challenge = Base64Url.Encode(options.Challenge);
        _memoryCache.Set($"fido2.attestationOptions.{challenge}", options.ToJson(), TimeSpan.FromMinutes(5));

        var totp = _totpProvider.GenerateTotpFor(Encoding.UTF8.GetBytes(command.Email));
        await _emailSender.SendConfirmationEmail(
            recipient: command.Email,
            subject: "Welcome to TruQuest!",
            body: $"Your confirmation code is {totp}"
        );

        return new()
        {
            Data = options
        };
    }
}
