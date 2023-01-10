using MediatR;

using Domain.Results;

using Application.Thing.Common.Models.IM;
using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Messages.Requests;

namespace Application.Thing.Commands.CreateNewThingDraft;

[RequireAuthorization]
public class CreateNewThingDraftCommand : IRequest<HandleResult<Guid>>
{
    public required NewThingIm Input { get; init; }
}

internal class CreateNewThingDraftCommandHandler : IRequestHandler<CreateNewThingDraftCommand, HandleResult<Guid>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IRequestDispatcher _requestDispatcher;

    public CreateNewThingDraftCommandHandler(
        ICurrentPrincipal currentPrincipal,
        IRequestDispatcher requestDispatcher
    )
    {
        _currentPrincipal = currentPrincipal;
        _requestDispatcher = requestDispatcher;
    }

    public async Task<HandleResult<Guid>> Handle(CreateNewThingDraftCommand command, CancellationToken ct)
    {
        var thingId = Guid.NewGuid();

        await _requestDispatcher.Send(new ArchiveThingAttachmentsCommand
        {
            SubmitterId = _currentPrincipal.Id,
            ThingId = thingId,
            Input = command.Input
        });

        return new()
        {
            Data = thingId
        };
    }
}