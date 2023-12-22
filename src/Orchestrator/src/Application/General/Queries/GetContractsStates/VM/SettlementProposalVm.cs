namespace Application.General.Queries.GetContractsStates.VM;

public class SettlementProposalVm
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public SettlementProposalAssessmentVerifierLotteryVm? Lottery { get; init; }
    public PollVm? Poll { get; init; }
}
