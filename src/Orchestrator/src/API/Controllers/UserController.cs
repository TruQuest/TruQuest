using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.User.Commands.SignUp;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ISender _mediator;

    public UserController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("signup")]
    public Task<HandleResult<SignUpResultVm>> SignUp(SignUpCommand command) => _mediator.Send(command);
}