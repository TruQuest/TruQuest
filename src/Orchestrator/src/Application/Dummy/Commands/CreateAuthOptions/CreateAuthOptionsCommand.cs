using Application.Dummy.Commands.CreateUser;
using Domain.Results;
using Fido2NetLib;
using Fido2NetLib.Objects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Dummy.Commands.CreateAuthOptions;

public class CreateAuthOptionsCommand : IRequest<HandleResult<AssertionOptions>>
{
    public required string Email { get; init; }
}

internal class CreateAuthOptionsCommandHandler : IRequestHandler<CreateAuthOptionsCommand, HandleResult<AssertionOptions>>
{
    private readonly ILogger<CreateAuthOptionsCommandHandler> _logger;
    private readonly DummyUserRepo _dummyUserRepo;
    private readonly IFido2 _fido2;

    public CreateAuthOptionsCommandHandler(
        ILogger<CreateAuthOptionsCommandHandler> logger,
        DummyUserRepo dummyUserRepo, IFido2 fido2
    )
    {
        _logger = logger;
        _dummyUserRepo = dummyUserRepo;
        _fido2 = fido2;
    }

    public async Task<HandleResult<AssertionOptions>> Handle(CreateAuthOptionsCommand command, CancellationToken ct)
    {
        var userId = _dummyUserRepo.GetByEmail(command.Email).Id;
        var existingCredentials = _dummyUserRepo.GetUserPublicKeyDescriptors(userId);

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
            existingCredentials,
            UserVerificationRequirement.Discouraged
        // exts
        );

        _logger.LogInformation(options.ToJson());

        _dummyUserRepo.SetAuthSession(userId, options);

        return new()
        {
            Data = options
        };
    }
}
