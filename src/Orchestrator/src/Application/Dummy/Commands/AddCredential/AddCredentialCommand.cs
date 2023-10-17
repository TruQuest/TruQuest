using Application.Dummy.Commands.CreateUser;
using Domain.Results;
using Fido2NetLib;
using Fido2NetLib.Objects;
using MediatR;

namespace Application.Dummy.Commands.AddCredential;

public class AddCredentialCommand : IRequest<VoidResult>
{
    public required Guid UserId { get; init; }
    public required AuthenticatorAttestationRawResponse Credential { get; init; }
}

internal class AddCredentialCommandHandler : IRequestHandler<AddCredentialCommand, VoidResult>
{
    private readonly DummyUserRepo _dummyUserRepo;
    private readonly IFido2 _fido2;

    public AddCredentialCommandHandler(DummyUserRepo dummyUserRepo, IFido2 fido2)
    {
        _dummyUserRepo = dummyUserRepo;
        _fido2 = fido2;
    }

    public async Task<VoidResult> Handle(AddCredentialCommand command, CancellationToken ct)
    {
        var options = CredentialCreateOptions.FromJson(_dummyUserRepo.GetRegSession(command.UserId));

        IsCredentialIdUniqueToUserAsyncDelegate uniqueCheck = (args, cancellationToken) =>
            Task.FromResult(_dummyUserRepo.CheckCredentialIsUniqueToSingleUser(args.CredentialId));

        var result = await _fido2.MakeNewCredentialAsync(command.Credential, options, uniqueCheck, cancellationToken: ct);

        var cred = new StoredCredential
        {
            Id = result.Result!.Id,
            Descriptor = new PublicKeyCredentialDescriptor(result.Result.Id),
            PublicKey = result.Result.PublicKey,
            UserHandle = result.Result.User.Id,
            SignCount = result.Result.SignCount,
            AttestationFormat = result.Result.AttestationFormat,
            RegDate = DateTime.Now,
            AaGuid = result.Result.AaGuid,
            Transports = result.Result.Transports,
            IsBackupEligible = result.Result.IsBackupEligible,
            IsBackedUp = result.Result.IsBackedUp,
            AttestationObject = result.Result.AttestationObject,
            AttestationClientDataJSON = result.Result.AttestationClientDataJson,
            DevicePublicKeys = new() { result.Result.DevicePublicKey }
        };

        _dummyUserRepo.AddCredentialToUser(options.User, cred);

        return VoidResult.Instance;
    }
}
