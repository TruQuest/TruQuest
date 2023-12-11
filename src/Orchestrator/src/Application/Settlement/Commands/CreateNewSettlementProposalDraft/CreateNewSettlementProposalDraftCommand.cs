using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Results;
using Domain.Aggregates;
using Domain.Errors;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Settlement.Common.Models.IM;
using Application.Common.Messages.Requests;
using Application.Common.Models.IM;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Settlement.Commands.CreateNewSettlementProposalDraft;

[RequireAuthorization]
public class CreateNewSettlementProposalDraftCommand : ManuallyBoundInputModelCommand, IRequest<HandleResult<Guid>>
{
    public CreateNewSettlementProposalDraftCommand(HttpRequest request) :
        base(request, new NewSettlementProposalIm())
    { }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(HandleResult<Guid> response)
    {
        if (response.Error == null)
        {
            return new (string, object?)[]
            {
                (ActivityTags.ThingId, ((NewSettlementProposalIm)Input).ThingId),
                (ActivityTags.SettlementProposalId, response.Data)
            };
        }

        return Enumerable.Empty<(string, object?)>();
    }
}

public class CreateNewSettlementProposalDraftCommandHandler :
    IRequestHandler<CreateNewSettlementProposalDraftCommand, HandleResult<Guid>>
{
    private readonly ILogger<CreateNewSettlementProposalDraftCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IThingQueryable _thingQueryable;
    private readonly IRequestDispatcher _requestDispatcher;

    public CreateNewSettlementProposalDraftCommandHandler(
        ILogger<CreateNewSettlementProposalDraftCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        IThingQueryable thingQueryable,
        IRequestDispatcher requestDispatcher
    )
    {
        _logger = logger;
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
            _logger.LogWarning(
                $"User {UserId} trying to create a settlement proposal for a thing {ThingId} that is not awaiting settlement",
                _currentPrincipal.Id!, input.ThingId
            );
            return new()
            {
                Error = new HandleError("The specified promise is not awaiting settlement")
            };
        }

        var proposalId = Guid.NewGuid();

        await _requestDispatcher.Send(
            new ArchiveSettlementProposalAttachmentsCommand
            {
                SubmitterId = _currentPrincipal.Id!,
                ProposalId = proposalId,
                Input = input
            },
            requestId: command.RequestId
        );

        return new()
        {
            Data = proposalId
        };
    }
}
