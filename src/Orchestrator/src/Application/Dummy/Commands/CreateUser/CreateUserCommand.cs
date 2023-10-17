using Domain.Results;
using Fido2NetLib;
using Fido2NetLib.Objects;
using MediatR;

namespace Application.Dummy.Commands.CreateUser;

public class DummyUser
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required bool EmailConfirmed { get; init; }
}

public class StoredCredential
{
    /// <summary>
    /// The Credential ID of the public key credential source.
    /// </summary>
    public byte[] Id { get; set; }

    /// <summary>
    /// The credential public key of the public key credential source.
    /// </summary>
    public byte[] PublicKey { get; set; }

    /// <summary>
    /// The latest value of the signature counter in the authenticator data from any ceremony using the public key credential source.
    /// </summary>
    public uint SignCount { get; set; }

    /// <summary>
    /// The value returned from getTransports() when the public key credential source was registered.
    /// </summary>
    public AuthenticatorTransport[] Transports { get; set; }

    /// <summary>
    /// The value of the BE flag when the public key credential source was created.
    /// </summary>
    public bool IsBackupEligible { get; set; }

    /// <summary>
    /// The latest value of the BS flag in the authenticator data from any ceremony using the public key credential source.
    /// </summary>
    public bool IsBackedUp { get; set; }

    /// <summary>
    /// The value of the attestationObject attribute when the public key credential source was registered. 
    /// Storing this enables the Relying Party to reference the credential's attestation statement at a later time.
    /// </summary>
    public byte[] AttestationObject { get; set; }

    public List<byte[]> DevicePublicKeys { get; set; }

    /// <summary>
    /// The value of the clientDataJSON attribute when the public key credential source was registered. 
    /// Storing this in combination with the above attestationObject item enables the Relying Party to re-verify the attestation signature at a later time.
    /// </summary>
    public byte[] AttestationClientDataJSON { get; set; }

    public byte[] UserId { get; set; }

    public PublicKeyCredentialDescriptor Descriptor { get; set; }

    public byte[] UserHandle { get; set; }

    public string AttestationFormat { get; set; }

    public DateTime RegDate { get; set; }

    public Guid AaGuid { get; set; }
}

public class DummyUserRepo
{
    private readonly List<DummyUser> _users = new();
    private readonly List<StoredCredential> _credentials = new();
    private readonly Dictionary<string, string> _session = new();

    public Guid Create(string email)
    {
        var user = new DummyUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            EmailConfirmed = true
        };
        _users.Add(user);

        return user.Id;
    }

    public DummyUser GetByEmail(string email) => _users.Single(u => u.Email == email);

    public List<PublicKeyCredentialDescriptor> GetUserPublicKeyDescriptors(Guid userId) => _credentials
        .Where(c => c.UserId.SequenceEqual(userId.ToByteArray()))
        .Select(c => c.Descriptor)
        .ToList();

    public StoredCredential GetCredentialById(byte[] credentialId) =>
        _credentials.Single(c => c.Descriptor.Id.SequenceEqual(credentialId));

    public bool CheckCredentialIsUniqueToSingleUser(byte[] credentialId)
    {
        return !_credentials.Any(c => c.Descriptor.Id.SequenceEqual(credentialId));
    }

    public void AddCredentialToUser(Fido2User user, StoredCredential credential)
    {
        credential.UserId = user.Id;
        _credentials.Add(credential);
    }

    public void SetRegSession(Guid userId, CredentialCreateOptions options)
    {
        _session.Add($"{userId}.fido2.attestationOptions", options.ToJson());
    }

    public string GetRegSession(Guid userId)
    {
        return _session[$"{userId}.fido2.attestationOptions"];
    }

    public void SetAuthSession(Guid userId, AssertionOptions options)
    {
        _session.Add($"{userId}.fido2.assertionOptions", options.ToJson());
    }

    public string GetAuthSession(Guid userId)
    {
        return _session[$"{userId}.fido2.assertionOptions"];
    }
}

public class CreateUserCommand : IRequest<HandleResult<Guid>>
{
    public required string Email { get; init; }
}

internal class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, HandleResult<Guid>>
{
    private readonly DummyUserRepo _dummyUserRepo;

    public CreateUserCommandHandler(DummyUserRepo dummyUserRepo)
    {
        _dummyUserRepo = dummyUserRepo;
    }

    public async Task<HandleResult<Guid>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var userId = _dummyUserRepo.Create(command.Email);
        return new()
        {
            Data = userId
        };
    }
}
