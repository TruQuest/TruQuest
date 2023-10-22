using Domain.Base;

namespace Domain.Aggregates;

public class AuthCredential : Entity
{
    public string Id { get; }
    public string UserId { get; set; }
    public string PublicKey { get; }
    public int SignCount { get; private set; }
    private List<int>? _transports;
    public IReadOnlyList<int>? Transports => _transports;
    public bool IsBackupEligible { get; }
    public bool IsBackedUp { get; }
    public string AttestationObject { get; }
    public string AttestationClientDataJSON { get; }
    public string AttestationFormat { get; }
    public long AddedAt { get; }
    public Guid AaGuid { get; } // @@??: Is this always 'AAA...'?

    public AuthCredential(
        string id, string userId, string publicKey, int signCount, bool isBackupEligible,
        bool isBackedUp, string attestationObject, string attestationClientDataJSON,
        string attestationFormat, long addedAt, Guid aaGuid
    )
    {
        Id = id;
        UserId = userId;
        PublicKey = publicKey;
        SignCount = signCount;
        IsBackupEligible = isBackupEligible;
        IsBackedUp = isBackedUp;
        AttestationObject = attestationObject;
        AttestationClientDataJSON = attestationClientDataJSON;
        AttestationFormat = attestationFormat;
        AddedAt = addedAt;
        AaGuid = aaGuid;
    }

    public void AddTransports(List<int>? transports) => _transports = transports;

    public void SetSignCount(int signCount) => SignCount = signCount;
}
