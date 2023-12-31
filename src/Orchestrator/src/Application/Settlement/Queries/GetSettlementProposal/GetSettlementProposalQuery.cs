using GoThataway;
using FluentValidation;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Queries.GetSettlementProposal;

public class GetSettlementProposalQuery : IRequest<HandleResult<GetSettlementProposalResultVm>>
{
    public required Guid ProposalId { get; init; }
}

internal class Validator : AbstractValidator<GetSettlementProposalQuery>
{
    public Validator()
    {
        RuleFor(q => q.ProposalId).NotEmpty();
    }
}

public class GetSettlementProposalQueryHandler :
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
        var proposal = await _settlementProposalQueryable.GetById(query.ProposalId, _currentPrincipal.Id);
        if (proposal == null)
        {
            return new()
            {
                Error = new HandleError("Not Found")
            };
        }
        if (proposal.State == SettlementProposalState.Draft && _currentPrincipal.Id != proposal.SubmitterId)
        {
            return new()
            {
                Error = new HandleError("Invalid request")
            };
        }

        string? signature = null;
        if (
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
