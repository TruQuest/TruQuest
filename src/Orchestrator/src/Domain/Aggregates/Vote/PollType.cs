namespace Domain.Aggregates;

public enum PollType
{
    Acceptance,
}

public static class PollTypeExtension
{
    public static string GetString(this PollType pollType) => pollType.ToString();
}