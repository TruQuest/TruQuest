using Microsoft.AspNetCore.Http;

using GoThataway;

using Domain.Results;
using Domain.Aggregates;
using Domain.Errors;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Settlement.Common.Models.IM;
using Application.Common.Messages.Requests;
using Application.Common.Models.IM;

namespace Application.Settlement.Commands.CreateNewSettlementProposalDraft;

[RequireAuthorization]
public class CreateNewSettlementProposalDraftCommand : ManuallyBoundInputModelCommand, IRequest<HandleResult<Guid>>
{
    public CreateNewSettlementProposalDraftCommand(HttpRequest request) :
        base(request, new NewSettlementProposalIm())
    { }
}

public class CreateNewSettlementProposalDraftCommandHandler :
    IRequestHandler<CreateNewSettlementProposalDraftCommand, HandleResult<Guid>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IThingQueryable _thingQueryable;
    private readonly IRequestDispatcher _requestDispatcher;

    public CreateNewSettlementProposalDraftCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IThingQueryable thingQueryable,
        IRequestDispatcher requestDispatcher
    )
    {
        _currentPrincipal = currentPrincipal;
        _thingQueryable = thingQueryable;
        _requestDispatcher = requestDispatcher;
    }

    public async Task<HandleResult<Guid>> Handle(CreateNewSettlementProposalDraftCommand command, CancellationToken ct)
    {
        // @@NOTE: We allow new proposals so long as the corresponding thing is still awaiting settlement, even if there
        // are other proposals that are in a more advanced stage, since they still could be rejected.

        var input = (NewSettlementProposalIm)command.Input;

        var thingState = await _thingQueryable.GetStateFor(input.ThingId);
        if (thingState != ThingState.AwaitingSettlement)
        {
            return new()
            {
                Error = new SettlementError("The specified promise is not awaiting settlement")
            };
        }

        var proposalId = Guid.NewGuid();

        await _requestDispatcher.Send(new ArchiveSettlementProposalAttachmentsCommand
        {
            SubmitterId = _currentPrincipal.Id!,
            ProposalId = proposalId,
            Input = input
        });

        return new()
        {
            Data = proposalId
        };
    }
}
