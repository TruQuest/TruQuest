using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Commands.CreateNewThingDraft;

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
}