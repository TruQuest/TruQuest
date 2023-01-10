using Microsoft.AspNetCore.SignalR;

using MediatR;

using Domain.Results;
using Application.Thing.Commands.CreateNewThingDraft;

using API.Hubs.Clients;
using API.Hubs.Filters;

namespace API.Hubs;

public class TruQuestHub : Hub<ITruQuestClient>
{
    private readonly ILogger<TruQuestHub> _logger;
    private readonly ISender _mediator;

    public TruQuestHub(ILogger<TruQuestHub> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"{Context.UserIdentifier} connected!");
        return base.OnConnectedAsync();
    }

    [CopyAuthenticationContextToMethodInvocationScope]
    public Task<HandleResult<Guid>> CreateNewThingDraft(CreateNewThingDraftCommand command) =>
        _mediator.Send(command);
}