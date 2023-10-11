namespace Application.Common.Models.QM;

public class ThingAssessmentPollResultQm
{
    public required SettlementProposalStateQm State { get; init; }
    public required string? VoteAggIpfsCid { get; init; }
}
