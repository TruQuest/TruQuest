using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Account.Commands.SignUp;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly ISender _mediator;

    public AccountController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("signup")]
    public Task<HandleResult<SignUpResultVm>> SignUp(SignUpCommand command) => _mediator.Send(command);
}