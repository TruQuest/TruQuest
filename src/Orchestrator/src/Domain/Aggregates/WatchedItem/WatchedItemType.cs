namespace Domain.Aggregates;

public enum WatchedItemType
{
    Subject,
    Thing,
    SettlementProposal,
}

public static class WatchedItemTypeExtension
{
    public static string GetString(this WatchedItemType itemType)
    {
        switch (itemType)
        {
            case WatchedItemType.Subject: return "subject";
            case WatchedItemType.Thing: return "thing";
            case WatchedItemType.SettlementProposal: return "settlement_proposal";
        }

        throw new InvalidOperationException();
    }
}
