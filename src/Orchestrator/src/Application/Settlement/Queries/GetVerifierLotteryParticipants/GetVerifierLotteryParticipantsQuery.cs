using System.Diagnostics;

using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsQuery : IRequest<HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    public required Guid ProposalId { get; init; }
}

internal class GetVerifierLotteryParticipantsQueryHandler :
    IRequestHandler<GetVerifierLotteryParticipantsQuery, HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    private readonly ISettlementProposalQueryable _settlementProposalQueryable;
    private readonly ISigner _signer;

    public GetVerifierLotteryParticipantsQueryHandler(
        ISettlementProposalQueryable settlementProposalQueryable,
        ISigner signer
    )
    {
        _settlementProposalQueryable = settlementProposalQueryable;
        _signer = signer;
    }

    public async Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> Handle(
        GetVerifierLotteryParticipantsQuery query, CancellationToken ct
    )
    {
        // @@TODO: Get all participants and claimants.
        var entries = await _settlementProposalQueryable.GetVerifierLotteryParticipants(query.ProposalId);
        var firstEntry = entries.FirstOrDefault();
        if (firstEntry != null && _signer.CheckIsOrchestrator(firstEntry.UserId))
        {
            // means the lottery was closed with success
            Debug.Assert(firstEntry.Nonce != null);
            firstEntry.IsOrchestrator = true;

            var verifiers = await _settlementProposalQueryable.GetVerifiers(query.ProposalId);
            Debug.Assert(verifiers.Any());
            foreach (var verifier in verifiers)
            {
                // @@BUG: If verifier is a claimant there wouldn't be a joined event.
                var entry = entries.Single(e => e.UserId == verifier.VerifierId);
                entry.IsWinner = true;
            }
        }

        entries = entries
            .OrderBy(e => e.SortKey)
            .ThenByDescending(e => e.JoinedBlockNumber);

        return new()
        {
            Data = new()
            {
                ProposalId = query.ProposalId,
                Entries = entries
            }
        };
    }
}