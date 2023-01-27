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

    public GetVerifierLotteryParticipantsQueryHandler(ISettlementProposalQueryable settlementProposalQueryable)
    {
        _settlementProposalQueryable = settlementProposalQueryable;
    }

    public async Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> Handle(GetVerifierLotteryParticipantsQuery query, CancellationToken ct)
    {
        var entries = await _settlementProposalQueryable.GetVerifierLotteryParticipants(
            query.ProposalId
        );

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