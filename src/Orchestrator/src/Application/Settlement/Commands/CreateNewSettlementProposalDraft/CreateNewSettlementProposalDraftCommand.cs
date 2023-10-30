using Microsoft.AspNetCore.Http;

using MediatR;

using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Settlement.Common.Models.IM;
using Application.Common.Messages.Requests;

namespace Application.Settlement.Commands.CreateNewSettlementProposalDraft;

[RequireAuthorization]
public class CreateNewSettlementProposalDraftCommand : IRequest<HandleResult<Guid>>
{
    public required HttpRequest Request { get; init; }
}

internal class CreateNewSettlementProposalDraftCommandHandler :
    IRequestHandler<CreateNewSettlementProposalDraftCommand, HandleResult<Guid>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IFileReceiver _fileReceiver;
    private readonly IRequestDispatcher _requestDispatcher;

    public CreateNewSettlementProposalDraftCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IFileReceiver fileReceiver,
        IRequestDispatcher requestDispatcher
    )
    {
        _currentPrincipal = currentPrincipal;
        _fileReceiver = fileReceiver;
        _requestDispatcher = requestDispatcher;
    }

    public async Task<HandleResult<Guid>> Handle(
        CreateNewSettlementProposalDraftCommand command, CancellationToken ct
    )
    {
        // @@TODO: Check that the thing is actually awaiting settlement.
        // @@TODO??: Check that there isn't an already funded proposal? Or allow new drafts event if there is one?

        var result = await _fileReceiver.ReceiveFilesAndFormValues(
            command.Request,
            maxSize: 10 * 1024 * 1024,
            filePrefix: _currentPrincipal.Id!
        );
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        var formValues = result.Data!;
        // @@TODO: Validate form.

        var input = new NewSettlementProposalIm
        {
            ThingId = Guid.Parse(formValues["thingId"]!),
            Title = formValues["title"]!,
            Verdict = (VerdictIm)int.Parse(formValues["verdict"]!),
            Details = formValues["details"]!,
            ImagePath = formValues["file1"],
            CroppedImagePath = formValues["file2"],
            Evidence = ((string)formValues["evidence"]!).Split('|')
                .Select(url => new SettlementProposalEvidenceIm
                {
                    Url = url
                })
                .ToList()
        };

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
