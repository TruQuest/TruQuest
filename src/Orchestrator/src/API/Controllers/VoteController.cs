using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Vote.Commands.CastAcceptancePollVote;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class VoteController : ControllerBase
{
    private readonly ISender _mediator;

    public VoteController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("acceptance/cast")]
    public Task<HandleResult<string>> Cast(CastAcceptancePollVoteCommand command) => _mediator.Send(command);
}