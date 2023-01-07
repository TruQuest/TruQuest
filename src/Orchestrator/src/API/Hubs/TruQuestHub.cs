using Microsoft.AspNetCore.SignalR;

using MediatR;

using Domain.Results;
using Application.Thing.Commands.CreateNewThingDraft;

using API.Hubs.Clients;
using API.Hubs.Filters;

namespace API.Hubs;

public class TruQuestHub : Hub<ITruQuestClient>
{
    private readonly ISender _mediator;

    public TruQuestHub(ISender mediator)
    {
        _mediator = mediator;
    }

    [CopyAuthenticationContextToMethodInvocationScope]
    public Task<HandleResult<Guid>> CreateNewThingDraft(CreateNewThingDraftCommand command) =>
        _mediator.Send(command);
}