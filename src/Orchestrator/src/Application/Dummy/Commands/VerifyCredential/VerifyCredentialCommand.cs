using Application.Common.Misc;
using Application.Dummy.Commands.CreateUser;
using Domain.Results;
using Fido2NetLib;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Dummy.Commands.VerifyCredential;

public class VerifyCredentialCommand : IRequest<VoidResult>
{
    public required string Email { get; init; }
    public required AuthenticatorAssertionRawResponse Credential { get; init; }
}

internal class VerifyCredentialCommandHandler : IRequestHandler<VerifyCredentialCommand, VoidResult>
{
    private readonly ILogger<VerifyCredentialCommandHandler> _logger;
    private readonly DummyUserRepo _dummyUserRepo;
    private readonly IFido2 _fido2;

    public VerifyCredentialCommandHandler(
        ILogger<VerifyCredentialCommandHandler> logger,
        DummyUserRepo dummyUserRepo, IFido2 fido2
    )
    {
        _logger = logger;
        _dummyUserRepo = dummyUserRepo;
        _fido2 = fido2;
    }

    public async Task<VoidResult> Handle(VerifyCredentialCommand command, CancellationToken ct)
    {
        var userId = _dummyUserRepo.GetByEmail(command.Email).Id;

        var options = AssertionOptions.FromJson(_dummyUserRepo.GetAuthSession(userId));

        var credential = _dummyUserRepo.GetCredentialById(command.Credential.Id);

        var storedCounter = credential.SignCount;

        IsUserHandleOwnerOfCredentialIdAsync callback = (args, cancellationToken) =>
        {
            return Task.FromResult(_dummyUserRepo.GetCredentialById(args.CredentialId).UserId.SequenceEqual(args.UserHandle));
        };

        var res = await _fido2.MakeAssertionAsync(
            command.Credential,
            options,
            credential.PublicKey,
            new(),
            storedCounter,
            callback,
            cancellationToken: ct
        );

        credential.SignCount = res.SignCount;

        _logger.LogInformation($"******************* {res.Status} {res.DevicePublicKey?.ToHex(prefix: true)} ***********************");

        return VoidResult.Instance;
    }
}
