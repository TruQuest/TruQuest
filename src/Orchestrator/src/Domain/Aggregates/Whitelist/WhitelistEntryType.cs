namespace Domain.Aggregates;

public enum WhitelistEntryType
{
    Email,
    SignerAddress
}

public static class WhitelistEntryTypeExtension
{
    public static string GetString(this WhitelistEntryType entryType)
    {
        switch (entryType)
        {
            case WhitelistEntryType.Email: return "email";
            case WhitelistEntryType.SignerAddress: return "signer_address";
        }

        throw new InvalidOperationException();
    }
}
