using Application.Dummy.Commands.CreateUser;
using Domain.Results;
using Fido2NetLib;
using Fido2NetLib.Objects;
using MediatR;

namespace Application.Dummy.Commands.CreateRegOptions;

public class CreateRegOptionsCommand : IRequest<HandleResult<CredentialCreateOptions>>
{
    public required string Email { get; init; }
}

internal class CreateRegOptionsCommandHandler : IRequestHandler<CreateRegOptionsCommand, HandleResult<CredentialCreateOptions>>
{
    private readonly DummyUserRepo _dummyUserRepo;
    private readonly IFido2 _fido2;

    public CreateRegOptionsCommandHandler(DummyUserRepo dummyUserRepo, IFido2 fido2)
    {
        _dummyUserRepo = dummyUserRepo;
        _fido2 = fido2;
    }

    public async Task<HandleResult<CredentialCreateOptions>> Handle(CreateRegOptionsCommand command, CancellationToken ct)
    {
        var user = _dummyUserRepo.GetByEmail(command.Email);
        var fido2User = new Fido2User
        {
            Id = user.Id.ToByteArray(),
            Name = user.Email,
            DisplayName = user.Email
        };

        var existingKeys = _dummyUserRepo.GetUserPublicKeyDescriptors(user.Id);

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
            existingKeys,
            authenticatorSelection,
            AttestationConveyancePreference.None
        // exts
        );

        _dummyUserRepo.SetRegSession(user.Id, options);

        return new()
        {
            Data = options
        };
    }
}
