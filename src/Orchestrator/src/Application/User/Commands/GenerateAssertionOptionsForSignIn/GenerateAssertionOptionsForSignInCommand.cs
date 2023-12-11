using Microsoft.Extensions.Caching.Memory;

using Fido2NetLib;
using Fido2NetLib.Objects;
using GoThataway;

using Domain.Results;

namespace Application.User.Commands.GenerateAssertionOptionsForSignIn;

public class GenerateAssertionOptionsForSignInCommand : IRequest<HandleResult<AssertionOptions>> { }

public class GenerateAssertionOptionsForSignInCommandHandler : IRequestHandler<
    GenerateAssertionOptionsForSignInCommand,
    HandleResult<AssertionOptions>
>
{
    private readonly IFido2 _fido2;
    private readonly IMemoryCache _memoryCache;

    public GenerateAssertionOptionsForSignInCommandHandler(IFido2 fido2, IMemoryCache memoryCache)
    {
        _fido2 = fido2;
        _memoryCache = memoryCache;
    }

    public Task<HandleResult<AssertionOptions>> Handle(GenerateAssertionOptionsForSignInCommand command, CancellationToken ct)
    {
        var options = _fido2.GetAssertionOptions(
            Enumerable.Empty<PublicKeyCredentialDescriptor>(),
            UserVerificationRequirement.Discouraged
        );

        _memoryCache.Set($"fido2.assertionOptions.{Base64Url.Encode(options.Challenge)}", options.ToJson(), TimeSpan.FromMinutes(5));

        return Task.FromResult(new HandleResult<AssertionOptions>()
        {
            Data = options
        });
    }
}
