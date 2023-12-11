using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

using GoThataway;
using Fido2NetLib;
using Fido2NetLib.Objects;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Interfaces;
using Application.User.Common.Models.VM;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.User.Commands.VerifyAssertionAndSignIn;

// @@TODO??: ExecuteInTxn?
public class VerifyAssertionAndSignInCommand : IRequest<HandleResult<AuthResultVm>>
{
    public required AuthenticatorAssertionRawResponse RawAssertion { get; init; }
}

public class VerifyAssertionAndSignInCommandHandler : IRequestHandler<
    VerifyAssertionAndSignInCommand,
    HandleResult<AuthResultVm>
>
{
    private readonly ILogger<VerifyAssertionAndSignInCommandHandler> _logger;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenProvider _authTokenProvider;

    public VerifyAssertionAndSignInCommandHandler(
        ILogger<VerifyAssertionAndSignInCommandHandler> logger,
        IFido2 fido2,
        IMemoryCache memoryCache,
        IUserRepository userRepository,
        IAuthTokenProvider authTokenProvider
    )
    {
        _logger = logger;
        _fido2 = fido2;
        _memoryCache = memoryCache;
        _userRepository = userRepository;
        _authTokenProvider = authTokenProvider;
    }

    public async Task<HandleResult<AuthResultVm>> Handle(VerifyAssertionAndSignInCommand command, CancellationToken ct)
    {
        var assertion = AuthenticatorAssertionResponse.Parse(command.RawAssertion);

        var cacheKey = $"fido2.assertionOptions.{Base64Url.Encode(assertion.Challenge)}";
        if (!_memoryCache.TryGetValue<string>(cacheKey, out string? optionsJson))
        {
            _logger.LogInformation($"User {UserId} provided assertion for challenge that has expired", new Guid(assertion.UserHandle!));
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
            _logger.LogInformation($"User {UserId} provided assertion using non-existent credential", new Guid(assertion.UserHandle!));
            return new()
            {
                Error = new HandleError("The specified credential not found")
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
            _logger.LogWarning(ex, $"Error trying to make assertion for user {UserId}", credential.UserId);
            return new()
            {
                Error = new HandleError("Invalid credential")
            };
        }

        credential.SetSignCount((int)result.SignCount);

        await _userRepository.SaveChanges();

        var claims = await _userRepository.GetClaimsExcept(credential.UserId, new[] { "key_share" });

        _logger.LogInformation($"User {UserId} signed-in", credential.UserId);

        return new()
        {
            Data = new()
            {
                UserId = credential.UserId,
                SignerAddress = claims.Single(c => c.Type == "signer_address").Value,
                WalletAddress = claims.Single(c => c.Type == "wallet_address").Value,
                Token = _authTokenProvider.GenerateJwt(credential.UserId, claims)
            }
        };
    }
}
