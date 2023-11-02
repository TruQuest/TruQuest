using Microsoft.AspNetCore.Http;

using MediatR;

using Domain.Results;

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

internal class CreateNewSettlementProposalDraftCommandHandler :
    IRequestHandler<CreateNewSettlementProposalDraftCommand, HandleResult<Guid>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IRequestDispatcher _requestDispatcher;

    public CreateNewSettlementProposalDraftCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IRequestDispatcher requestDispatcher
    )
    {
        _currentPrincipal = currentPrincipal;
        _requestDispatcher = requestDispatcher;
    }

    public async Task<HandleResult<Guid>> Handle(
        CreateNewSettlementProposalDraftCommand command, CancellationToken ct
    )
    {
        // @@TODO: Check that the thing is actually awaiting settlement.
        // @@TODO??: Check that there isn't an already funded proposal? Or allow new drafts event if there is one?

        var proposalId = Guid.NewGuid();

        await _requestDispatcher.Send(new ArchiveSettlementProposalAttachmentsCommand
        {
            SubmitterId = _currentPrincipal.Id!,
            ProposalId = proposalId,
            Input = (NewSettlementProposalIm)command.Input
        });

        return new()
        {
            Data = proposalId
        };
    }
}
