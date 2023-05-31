namespace Application.Thing.Commands.CastAcceptancePollVote;

public enum DecisionIm
{
    SoftDecline,
    HardDecline,
    Accept,
}

public static class DecisionImExtension
{
    public static string GetString(this DecisionIm decision)
    {
        switch (decision)
        {
            case DecisionIm.SoftDecline:
                return "Soft decline";
            case DecisionIm.HardDecline:
                return "Hard decline";
            case DecisionIm.Accept:
                return "Accept";
        }

        throw new InvalidOperationException();
    }

    public static DecisionIm FromString(string decision)
    {
        switch (decision)
        {
            case "Soft decline":
                return DecisionIm.SoftDecline;
            case "Hard decline":
                return DecisionIm.HardDecline;
            case "Accept":
                return DecisionIm.Accept;
        }

        throw new InvalidOperationException();
    }
}