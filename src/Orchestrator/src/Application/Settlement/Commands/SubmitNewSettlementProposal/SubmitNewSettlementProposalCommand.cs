using Microsoft.Extensions.Logging;

using GoThataway;
using FluentValidation;

using Domain.Results;
using Domain.Aggregates;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

[RequireAuthorization, ExecuteInTxn]
public class SubmitNewSettlementProposalCommand : IRequest<HandleResult<SubmitNewSettlementProposalResultVm>>
{
    public required Guid ProposalId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(HandleResult<SubmitNewSettlementProposalResultVm> _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.SettlementProposalId, ProposalId)
        };
    }
}

internal class Validator : AbstractValidator<SubmitNewSettlementProposalCommand>
{
    public Validator()
    {
        RuleFor(c => c.ProposalId).NotEmpty();
    }
}

public class SubmitNewSettlementProposalCommandHandler :
    IRequestHandler<SubmitNewSettlementProposalCommand, HandleResult<SubmitNewSettlementProposalResultVm>>
{
    private readonly ILogger<SubmitNewSettlementProposalCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IThingQueryable _thingQueryable;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;

    public SubmitNewSettlementProposalCommandHandler(
        ILogger<SubmitNewSettlementProposalCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IThingQueryable thingQueryable,
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _thingQueryable = thingQueryable;
        _settlementProposalRepository = settlementProposalRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
    }

    public async Task<HandleResult<SubmitNewSettlementProposalResultVm>> Handle(
        SubmitNewSettlementProposalCommand command, CancellationToken ct
    )
    {
        var proposal = await _settlementProposalRepository.FindById(command.ProposalId);
        if (proposal.SubmitterId != _currentPrincipal.Id!)
        {
            _logger.LogWarning(
                $"User {UserId} trying to submit a settlement proposal {SettlementProposalId} created by another",
                _currentPrincipal.Id!, proposal.Id
            );
            return new()
            {
                Error = new HandleError("Invalid request")
            };
        }
        if (proposal.State != SettlementProposalState.Draft)
        {
            _logger.LogWarning(
                $"User {UserId} trying to submit an already submitted settlement proposal {SettlementProposalId}",
                _currentPrincipal.Id!, proposal.Id
            );
            return new()
            {
                Error = new HandleError("Already submitted")
            };
        }

        var thingState = await _thingQueryable.GetStateFor(proposal.ThingId);
        if (thingState != ThingState.AwaitingSettlement)
        {
            _logger.LogWarning(
                $"User {UserId} trying to submit a settlement proposal {SettlementProposalId} for a thing {ThingId} that is not awaiting settlement",
                _currentPrincipal.Id!, proposal.Id, proposal.ThingId
            );
            return new()
            {
                Error = new HandleError("The specified promise is not awaiting settlement")
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

        _logger.LogInformation($"Settlement proposal {SettlementProposalId} successfully submitted", proposal.Id);

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
