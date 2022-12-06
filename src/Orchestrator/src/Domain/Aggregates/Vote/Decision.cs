namespace Domain.Aggregates;

public enum Decision
{
    SoftDecline,
    HardDecline,
    Accept,
}

public static class DecisionExtension
{
    public static string GetString(this Decision decision)
    {
        switch (decision)
        {
            case Decision.SoftDecline:
                return "Soft decline";
            case Decision.HardDecline:
                return "Hard decline";
            case Decision.Accept:
                return "Accept";
        }

        throw new InvalidOperationException();
    }
}