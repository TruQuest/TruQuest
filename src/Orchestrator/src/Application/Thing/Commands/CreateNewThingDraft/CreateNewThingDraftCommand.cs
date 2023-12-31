using Microsoft.AspNetCore.Http;

using GoThataway;

using Domain.Results;

using Application.Thing.Common.Models.IM;
using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Messages.Requests;
using Application.Common.Models.IM;
using Application.Common.Monitoring;

namespace Application.Thing.Commands.CreateNewThingDraft;

[RequireAuthorization]
public class CreateNewThingDraftCommand : ManuallyBoundInputModelCommand, IRequest<HandleResult<Guid>>
{
    public CreateNewThingDraftCommand(HttpRequest request) : base(request, new NewThingIm()) { }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(HandleResult<Guid> response)
    {
        if (response.Error == null)
        {
            return new (string, object?)[]
            {
                (ActivityTags.SubjectId, ((NewThingIm)Input).SubjectId),
                (ActivityTags.ThingId, response.Data)
            };
        }

        return Enumerable.Empty<(string, object?)>();
    }
}

public class CreateNewThingDraftCommandHandler : IRequestHandler<CreateNewThingDraftCommand, HandleResult<Guid>>
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

        await _requestDispatcher.Send(
            new ArchiveThingAttachmentsCommand
            {
                SubmitterId = _currentPrincipal.Id!,
                ThingId = thingId,
                Input = (NewThingIm)command.Input
            },
            requestId: command.RequestId
        );

        return new()
        {
            Data = thingId
        };
    }
}
