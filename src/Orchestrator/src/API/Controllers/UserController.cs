using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.User.Commands.MarkNotificationsAsRead;
using Application.User.Queries.GetNonceForSiwe;
using Application.User.Commands.SignInWithEthereum;

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

    [HttpGet("siwe/{address}")]
    public Task<HandleResult<string>> GetNonceForSiwe([FromRoute] GetNonceForSiweQuery query) => _mediator.Send(query);

    [HttpPost("siwe")]
    public Task<HandleResult<SignInWithEthereumResultVm>> SignInWithEthereum(SignInWithEthereumCommand command)
        => _mediator.Send(command);

    [HttpPost("watch-list")]
    public Task<VoidResult> MarkNotificationsAsRead(MarkNotificationsAsReadCommand command) => _mediator.Send(command);
}