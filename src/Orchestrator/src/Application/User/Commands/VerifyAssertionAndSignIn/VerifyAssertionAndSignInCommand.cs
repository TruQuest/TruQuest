using Microsoft.Extensions.Caching.Memory;

using Fido2NetLib;
using GoThataway;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Interfaces;
using Application.User.Common.Models.VM;

namespace Application.User.Commands.VerifyAssertionAndSignIn;

public class VerifyAssertionAndSignInCommand : IRequest<HandleResult<AuthResultVm>>
{
    public required AuthenticatorAssertionRawResponse RawAssertion { get; init; }
}

public class VerifyAssertionAndSignInCommandHandler : IRequestHandler<
    VerifyAssertionAndSignInCommand,
    HandleResult<AuthResultVm>
>
{
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenProvider _authTokenProvider;

    public VerifyAssertionAndSignInCommandHandler(
        IFido2 fido2,
        IMemoryCache memoryCache,
        IUserRepository userRepository,
        IAuthTokenProvider authTokenProvider
    )
    {
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
            return new()
            {
                Error = new UserError("Challenge expired")
            };
        }

        _memoryCache.Remove(cacheKey);
        var options = AssertionOptions.FromJson(optionsJson);

        var credential = await _userRepository.GetAuthCredential(Base64Url.Encode(command.RawAssertion.Id));
        if (credential == null)
        {
            return new()
            {
                Error = new UserError("The specified credential not found")
            };
        }

        IsUserHandleOwnerOfCredentialIdAsync checkUserOwnsCredential = (args, ct) =>
            Task.FromResult(credential.UserId == new Guid(args.UserHandle).ToString());

        // @@TODO!!: Handle exceptions.
        var result = await _fido2.MakeAssertionAsync(
            command.RawAssertion,
            options,
            Base64Url.Decode(credential.PublicKey),
            new(),
            (uint)credential.SignCount,
            checkUserOwnsCredential,
            cancellationToken: ct
        );

        credential.SetSignCount((int)result.SignCount);

        await _userRepository.SaveChanges();

        var claims = await _userRepository.GetClaimsExcept(credential.UserId, new[] { "key_share" });

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
