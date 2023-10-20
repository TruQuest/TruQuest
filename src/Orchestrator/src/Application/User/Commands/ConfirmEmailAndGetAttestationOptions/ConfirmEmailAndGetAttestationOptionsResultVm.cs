using Fido2NetLib;

namespace Application.User.Commands.ConfirmEmailAndGetAttestationOptions;

public class ConfirmEmailAndGetAttestationOptionsResultVm
{
    public required CredentialCreateOptions Options { get; init; }
    public required string Nonce { get; init; }
}
