using Application.Common.Models.QM;

namespace Application.Settlement.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsResultVm
{
    public required Guid ProposalId { get; init; }
    public required IEnumerable<VerifierLotteryParticipantEntryQm> Entries { get; init; }
}