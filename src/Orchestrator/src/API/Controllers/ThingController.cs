using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Commands.CreateNewThingDraft;
using Application.Thing.Queries.GetThing;
using Application.Thing.Queries.GetVerifierLotteryParticipants;
using Application.Thing.Queries.GetVerifiers;
using Application.Thing.Commands.CastAcceptancePollVote;

using API.Controllers.Filters;

namespace API.Controllers;

[Route("things")]
public class ThingController : ControllerBase
{
    private readonly ISender _mediator;

    public ThingController(ISender mediator)
    {
        _mediator = mediator;
    }

    [DisableFormValueModelBinding]
    [RequestSizeLimit(10 * 1024 * 1024)] // @@TODO: Config.
    [HttpPost("draft")]
    public Task<HandleResult<Guid>> CreateNewThingDraft() => _mediator.Send(new CreateNewThingDraftCommand
    {
        Request = HttpContext.Request
    });

    [HttpPost("submit")]
    public Task<HandleResult<SubmitNewThingResultVm>> SubmitNewThing([FromBody] SubmitNewThingCommand command)
        => _mediator.Send(command);

    [HttpGet("{id}")]
    public Task<HandleResult<GetThingResultVm>> Get(string id) =>
        _mediator.Send(new GetThingQuery { ThingId = Guid.Parse(id) });

    [HttpGet("{id}/lottery-participants")]
    public Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> GetVerifierLotteryParticipants(string id) =>
        _mediator.Send(new GetVerifierLotteryParticipantsQuery { ThingId = Guid.Parse(id) });

    [HttpGet("{id}/verifiers")]
    public Task<HandleResult<GetVerifiersResultVm>> GetVerifiers(string id) =>
        _mediator.Send(new GetVerifiersQuery { ThingId = Guid.Parse(id) });

    [HttpPost("{id}/vote")]
    public Task<HandleResult<string>> CastAcceptancePollVote(
        [FromRoute] string id, [FromBody] CastAcceptancePollVoteCommand command
    )
    {
        command.Input.ThingId = Guid.Parse(id);
        return _mediator.Send(command);
    }
}