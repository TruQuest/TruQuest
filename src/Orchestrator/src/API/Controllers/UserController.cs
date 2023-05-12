using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.User.Commands.SignUp;
using Application.User.Queries.GetSignInData;
using Application.User.Commands.SignIn;
using Application.User.Commands.MarkNotificationsAsRead;

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

    [HttpPost("sign-up")]
    public Task<HandleResult<SignUpResultVm>> SignUp(SignUpCommand command) => _mediator.Send(command);

    [HttpGet("sign-in")]
    public Task<HandleResult<GetSignInDataResultVm>> SignIn() => _mediator.Send(new GetSignInDataQuery());

    [HttpPost("sign-in")]
    public Task<HandleResult<SignInResultVm>> SignIn(SignInCommand command) => _mediator.Send(command);

    [HttpPost("watch-list")]
    public Task<VoidResult> MarkNotificationsAsRead(MarkNotificationsAsReadCommand command) => _mediator.Send(command);
}