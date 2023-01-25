using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

[RequireAuthorization]
public class SubmitNewSettlementProposalCommand : IRequest<HandleResult<SubmitNewSettlementProposalResultVm>>
{
    public required Guid ProposalId { get; init; }
}

internal class SubmitNewSettlementProposalCommandHandler :
    IRequestHandler<SubmitNewSettlementProposalCommand, HandleResult<SubmitNewSettlementProposalResultVm>>
{
    private readonly ILogger<SubmitNewSettlementProposalCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly ISettlementProposalRepository _settlementProposalRepository;

    public SubmitNewSettlementProposalCommandHandler(
        ILogger<SubmitNewSettlementProposalCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        ISettlementProposalRepository settlementProposalRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _settlementProposalRepository = settlementProposalRepository;
    }

    public async Task<HandleResult<SubmitNewSettlementProposalResultVm>> Handle(
        SubmitNewSettlementProposalCommand command, CancellationToken ct
    )
    {
        var proposal = await _settlementProposalRepository.FindById(command.ProposalId);
        // @@??: Should check using resource-based authorization?
        if (proposal.SubmitterId != _currentPrincipal.Id!)
        {
            return new()
            {
                Error = new SettlementError("Invalid request")
            };
        }
        if (proposal.State != SettlementProposalState.Draft)
        {
            return new()
            {
                Error = new SettlementError("Already submitted")
            };
        }

        proposal.SetState(SettlementProposalState.AwaitingFunding);
        await _settlementProposalRepository.SaveChanges();

        return new()
        {
            Data = new()
            {
                ThingId = proposal.ThingId,
                ProposalId = proposal.Id,
                Signature = _signer.SignSettlementProposal(proposal.ThingId, proposal.Id)
            }
        };
    }
}