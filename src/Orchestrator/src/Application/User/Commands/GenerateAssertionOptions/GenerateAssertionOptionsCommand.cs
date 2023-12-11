using Microsoft.Extensions.Caching.Memory;

using Fido2NetLib;
using Fido2NetLib.Objects;
using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.User.Commands.GenerateAssertionOptions;

[RequireAuthorization]
public class GenerateAssertionOptionsCommand : IRequest<HandleResult<AssertionOptions>> { }

public class GenerateAssertionOptionsCommandHandler : IRequestHandler<GenerateAssertionOptionsCommand, HandleResult<AssertionOptions>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IUserRepository _userRepository;
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;

    public GenerateAssertionOptionsCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IUserRepository userRepository,
        IFido2 fido2,
        IMemoryCache memoryCache
    )
    {
        _currentPrincipal = currentPrincipal;
        _userRepository = userRepository;
        _fido2 = fido2;
        _memoryCache = memoryCache;
    }

    public async Task<HandleResult<AssertionOptions>> Handle(GenerateAssertionOptionsCommand command, CancellationToken ct)
    {
        // @@TODO: Use queryable.
        var allowedCredentials = (await _userRepository.GetAuthCredentialDescriptorsFor(_currentPrincipal.Id!))
            .Select(c => new PublicKeyCredentialDescriptor(
                PublicKeyCredentialType.PublicKey,
                Base64Url.Decode(c.Id),
                c.Transports?.Select(t => (AuthenticatorTransport)t).ToArray()
            ));

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

        var options = _fido2.GetAssertionOptions(
            allowedCredentials,
            UserVerificationRequirement.Discouraged
        );

        _memoryCache.Set($"fido2.assertionOptions.{Base64Url.Encode(options.Challenge)}", options.ToJson(), TimeSpan.FromMinutes(5));

        return new()
        {
            Data = options
        };
    }
}
