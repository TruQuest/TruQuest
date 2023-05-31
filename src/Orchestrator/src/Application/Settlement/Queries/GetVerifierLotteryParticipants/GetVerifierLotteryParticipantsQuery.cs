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
        var entries = await _settlementProposalQueryable.GetVerifierLotteryParticipants(
            query.ProposalId
        );
        var firstEntry = entries.First();
        if (_signer.CheckIsOrchestrator(firstEntry.UserId) && firstEntry.Nonce != null)
        {
            // means the lottery is completed successfully
            firstEntry.IsOrchestrator = true;
            var verifiers = await _settlementProposalQueryable.GetVerifiers(query.ProposalId);
            if (verifiers.Any())
            {
                // means it was completed with success
                foreach (var entry in entries)
                {
                    if (verifiers.Any(v => v.VerifierId == entry.UserId))
                    {
                        entry.IsWinner = true;
                    }
                }
            }
        }
        else
        {
            // means the lottery is still in progress or has failed
            foreach (var entry in entries.Where(e => e.Nonce == null))
            {
                if (_signer.CheckIsOrchestrator(entry.UserId))
                {
                    entry.IsOrchestrator = true;
                    break;
                }
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