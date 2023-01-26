using MediatR;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Queries.GetSettlementProposal;

public class GetSettlementProposalQuery : IRequest<HandleResult<GetSettlementProposalResultVm>>
{
    public required Guid ProposalId { get; init; }
}

internal class GetSettlementProposalQueryHandler :
    IRequestHandler<GetSettlementProposalQuery, HandleResult<GetSettlementProposalResultVm>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISettlementProposalQueryable _settlementProposalQueryable;
    private readonly ISigner _signer;

    public GetSettlementProposalQueryHandler(
        ICurrentPrincipal currentPrincipal,
        ISettlementProposalQueryable settlementProposalQueryable,
        ISigner signer
    )
    {
        _currentPrincipal = currentPrincipal;
        _settlementProposalQueryable = settlementProposalQueryable;
        _signer = signer;
    }

    public async Task<HandleResult<GetSettlementProposalResultVm>> Handle(
        GetSettlementProposalQuery query, CancellationToken ct
    )
    {
        var proposal = await _settlementProposalQueryable.GetById(query.ProposalId);
        if (proposal == null)
        {
            return new()
            {
                Error = new SettlementError("Not Found")
            };
        }
        if (proposal.State == SettlementProposalState.Draft && _currentPrincipal.Id != proposal.SubmitterId)
        {
            return new()
            {
                Error = new SettlementError("Invalid request")
            };
        }

        string? signature = null;
        if (
            _currentPrincipal.Id != null &&
            proposal.SubmitterId == _currentPrincipal.Id &&
            proposal.State == SettlementProposalState.AwaitingFunding
        )
        {
            signature = _signer.SignSettlementProposal(proposal.ThingId, proposal.Id);
        }

        return new()
        {
            Data = new()
            {
                Proposal = proposal,
                Signature = signature
            }
        };
    }
}