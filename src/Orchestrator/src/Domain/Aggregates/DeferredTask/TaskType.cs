namespace Domain.Aggregates;

public enum TaskType
{
    CloseThingValidationVerifierLottery,
    CloseThingValidationPoll,
    CloseSettlementProposalAssessmentVerifierLottery,
    CloseSettlementProposalAssessmentPoll,
}

public static class TaskTypeExtension
{
    public static string GetString(this TaskType type)
    {
        switch (type)
        {
            case TaskType.CloseThingValidationVerifierLottery:
                return "close_thing_validation_verifier_lottery";
            case TaskType.CloseThingValidationPoll:
                return "close_thing_validation_poll";
            case TaskType.CloseSettlementProposalAssessmentVerifierLottery:
                return "close_settlement_proposal_assessment_verifier_lottery";
            case TaskType.CloseSettlementProposalAssessmentPoll:
                return "close_settlement_proposal_assessment_poll";
        }

        throw new InvalidOperationException();
    }
}
