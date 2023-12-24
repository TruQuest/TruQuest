namespace Application.Admin.Queries.GetContractsStates.VM;

public class ThingVm
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public ThingValidationVerifierLotteryVm? Lottery { get; init; }
    public PollVm? Poll { get; init; }
    public SettlementProposalVm? SettlementProposal { get; init; }
}
