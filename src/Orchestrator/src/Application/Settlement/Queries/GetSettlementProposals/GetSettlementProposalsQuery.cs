using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Queries.GetSettlementProposals;

public class GetSettlementProposalsQuery : IRequest<HandleResult<GetSettlementProposalsResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class GetSettlementProposalsQueryHandler :
    IRequestHandler<GetSettlementProposalsQuery, HandleResult<GetSettlementProposalsResultVm>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISettlementProposalQueryable _settlementProposalQueryable;

    public GetSettlementProposalsQueryHandler(
        ICurrentPrincipal currentPrincipal,
        ISettlementProposalQueryable settlementProposalQueryable
    )
    {
        _currentPrincipal = currentPrincipal;
        _settlementProposalQueryable = settlementProposalQueryable;
    }

    public async Task<HandleResult<GetSettlementProposalsResultVm>> Handle(
        GetSettlementProposalsQuery query, CancellationToken ct
    )
    {
        var proposals = await (_currentPrincipal.Id != null ?
            _settlementProposalQueryable.GetAllExceptDraftsOfOthersFor(query.ThingId, _currentPrincipal.Id) :
            _settlementProposalQueryable.GetAllExceptDraftsFor(query.ThingId)
        );

        return new()
        {
            Data = new()
            {
                ThingId = query.ThingId,
                Proposals = proposals
            }
        };
    }
}