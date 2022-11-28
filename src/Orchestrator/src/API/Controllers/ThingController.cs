using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Thing.Commands.SubmitNewThing;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ThingController : ControllerBase
{
    private readonly ISender _mediator;

    public ThingController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("submit")]
    public Task<HandleResult<SubmitNewThingResultVM>> SubmitNewThing(SubmitNewThingCommand command)
        => _mediator.Send(command);
}