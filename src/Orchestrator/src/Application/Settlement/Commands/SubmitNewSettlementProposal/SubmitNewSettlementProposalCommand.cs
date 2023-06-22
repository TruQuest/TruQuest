using Microsoft.Extensions.Logging;

using MediatR;
using FluentValidation;

using Domain.Results;
using Domain.Aggregates;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

[RequireAuthorization, ExecuteInTxn]
public class SubmitNewSettlementProposalCommand : IRequest<HandleResult<SubmitNewSettlementProposalResultVm>>
{
    public required Guid ProposalId { get; init; }
}

internal class Validator : AbstractValidator<SubmitNewSettlementProposalCommand>
{
    public Validator()
    {
        RuleFor(c => c.ProposalId).NotEmpty();
    }
}

internal class SubmitNewSettlementProposalCommandHandler :
    IRequestHandler<SubmitNewSettlementProposalCommand, HandleResult<SubmitNewSettlementProposalResultVm>>
{
    private readonly ILogger<SubmitNewSettlementProposalCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;

    public SubmitNewSettlementProposalCommandHandler(
        ILogger<SubmitNewSettlementProposalCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _settlementProposalRepository = settlementProposalRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
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

        await _settlementProposalUpdateRepository.AddOrUpdate(new SettlementProposalUpdate(
            settlementProposalId: proposal.Id,
            category: SettlementProposalUpdateCategory.General,
            updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            title: "Proposal submitted",
            details: "Click to refresh the page"
        ));

        await _settlementProposalRepository.SaveChanges();
        await _settlementProposalUpdateRepository.SaveChanges();

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