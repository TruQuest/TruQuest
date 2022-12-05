using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Vote.Commands.CastVote;

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

    [HttpPost("cast")]
    public Task<HandleResult<string>> Cast(CastVoteCommand command) => _mediator.Send(command);
}