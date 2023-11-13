using GoThataway;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Thing.Queries.GetSettlementProposalsList;

public class GetSettlementProposalsListQuery : IRequest<HandleResult<GetSettlementProposalsListResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class Validator : AbstractValidator<GetSettlementProposalsListQuery>
{
    public Validator()
    {
        RuleFor(q => q.ThingId).NotEmpty();
    }
}

public class GetSettlementProposalsListQueryHandler :
    IRequestHandler<GetSettlementProposalsListQuery, HandleResult<GetSettlementProposalsListResultVm>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISettlementProposalQueryable _settlementProposalQueryable;

    public GetSettlementProposalsListQueryHandler(
        ICurrentPrincipal currentPrincipal,
        ISettlementProposalQueryable settlementProposalQueryable
    )
    {
        _currentPrincipal = currentPrincipal;
        _settlementProposalQueryable = settlementProposalQueryable;
    }

    public async Task<HandleResult<GetSettlementProposalsListResultVm>> Handle(
        GetSettlementProposalsListQuery query, CancellationToken ct
    )
    {
        var proposals = await _settlementProposalQueryable.GetForThing(query.ThingId, _currentPrincipal.Id);
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
