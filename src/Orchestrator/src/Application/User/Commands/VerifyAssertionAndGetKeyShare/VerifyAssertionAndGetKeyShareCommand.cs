using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

using GoThataway;
using Fido2NetLib;
using Fido2NetLib.Objects;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.User.Commands.VerifyAssertionAndGetKeyShare;

[RequireAuthorization] // @@TODO??: ExecuteInTxn?
public class VerifyAssertionAndGetKeyShareCommand : IRequest<HandleResult<string>>
{
    public required AuthenticatorAssertionRawResponse RawAssertion { get; init; }
}

public class VerifyAssertionAndGetKeyShareCommandHandler : IRequestHandler<
    VerifyAssertionAndGetKeyShareCommand,
    HandleResult<string>
>
{
    private readonly ILogger<VerifyAssertionAndGetKeyShareCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly IUserRepository _userRepository;

    public VerifyAssertionAndGetKeyShareCommandHandler(
        ILogger<VerifyAssertionAndGetKeyShareCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        IFido2 fido2,
        IMemoryCache memoryCache,
        IUserRepository userRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _fido2 = fido2;
        _memoryCache = memoryCache;
        _userRepository = userRepository;
    }

    public async Task<HandleResult<string>> Handle(VerifyAssertionAndGetKeyShareCommand command, CancellationToken ct)
    {
        var assertion = AuthenticatorAssertionResponse.Parse(command.RawAssertion);

        var cacheKey = $"fido2.assertionOptions.{Base64Url.Encode(assertion.Challenge)}";
        if (!_memoryCache.TryGetValue<string>(cacheKey, out string? optionsJson))
        {
            _logger.LogInformation($"User {UserId} provided assertion for challenge that has expired", _currentPrincipal.Id!);
            return new()
            {
                Error = new HandleError("Challenge expired")
            };
        }

        _memoryCache.Remove(cacheKey);
        var options = AssertionOptions.FromJson(optionsJson);

        var credential = await _userRepository.GetAuthCredential(Base64Url.Encode(command.RawAssertion.Id));
        if (credential == null)
        {
            _logger.LogInformation($"User {UserId} provided assertion using non-existent credential", _currentPrincipal.Id!);
            return new()
            {
                Error = new HandleError("The specified credential not found")
            };
        }

        if (_currentPrincipal.Id! != credential.UserId)
        {
            _logger.LogWarning(
                $"Current principal {UserId} and credential's owner {CredentialOwnerId} do not match",
                _currentPrincipal.Id!, credential.UserId
            );
            return new()
            {
                Error = new HandleError("Invalid request")
            };
        }

        IsUserHandleOwnerOfCredentialIdAsync checkUserOwnsCredential = (args, ct) =>
            Task.FromResult(credential.UserId == new Guid(args.UserHandle).ToString());

        VerifyAssertionResult result;
        try
        {
            result = await _fido2.MakeAssertionAsync(
                command.RawAssertion,
                options,
                Base64Url.Decode(credential.PublicKey),
                new(),
                (uint)credential.SignCount,
                checkUserOwnsCredential,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error trying to make assertion for user {UserId}", _currentPrincipal.Id!);
            return new()
            {
                Error = new HandleError("Invalid credential")
            };
        }

        credential.SetSignCount((int)result.SignCount);

        await _userRepository.SaveChanges();

        var keyShareClaim = await _userRepository.GetClaim(_currentPrincipal.Id!, "key_share");

        return new()
        {
            Data = keyShareClaim.Value
        };
    }
}
