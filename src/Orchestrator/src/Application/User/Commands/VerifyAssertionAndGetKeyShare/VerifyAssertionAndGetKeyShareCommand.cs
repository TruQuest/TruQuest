using Microsoft.Extensions.Caching.Memory;

using Fido2NetLib;
using MediatR;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.User.Commands.VerifyAssertionAndGetKeyShare;

[RequireAuthorization]
public class VerifyAssertionAndGetKeyShareCommand : IRequest<HandleResult<string>>
{
    public required AuthenticatorAssertionRawResponse RawAssertion { get; init; }
}

internal class VerifyAssertionAndGetKeyShareCommandHandler : IRequestHandler<
    VerifyAssertionAndGetKeyShareCommand,
    HandleResult<string>
>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;
    private readonly IUserRepository _userRepository;

    public VerifyAssertionAndGetKeyShareCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IFido2 fido2,
        IMemoryCache memoryCache,
        IUserRepository userRepository
    )
    {
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
            return new()
            {
                Error = new UserError("Challenge expired")
            };
        }

        _memoryCache.Remove(cacheKey);
        var options = AssertionOptions.FromJson(optionsJson);

        var credential = await _userRepository.GetUserAuthCredential(
            _currentPrincipal.Id!, Base64Url.Encode(command.RawAssertion.Id)
        );

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

        var keyShareClaim = await _userRepository.GetClaim(_currentPrincipal.Id!, "key_share");

        return new()
        {
            Data = keyShareClaim.Value
        };
    }
}
