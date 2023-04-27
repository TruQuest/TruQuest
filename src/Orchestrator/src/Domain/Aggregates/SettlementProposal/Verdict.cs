namespace Domain.Aggregates;

public enum Verdict
{
    Delivered,
    GuessItCounts,
    AintGoodEnough,
    MotionNotAction,
    NoEffortWhatsoever,
    AsGoodAsMaliciousIntent,
}

public static class VerdictExtension
{
    public static int GetScore(this Verdict verdict)
    {
        switch (verdict)
        {
            case Verdict.Delivered: return 100;
            case Verdict.GuessItCounts: return 75;
            case Verdict.AintGoodEnough: return 40;
            case Verdict.MotionNotAction: return 0;
            case Verdict.NoEffortWhatsoever: return -40;
            case Verdict.AsGoodAsMaliciousIntent: return -100;
        }

        throw new NotImplementedException();
    }
}