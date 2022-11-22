using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Subject.Commands.AddNewSubject;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class SubjectController : ControllerBase
{
    private readonly ISender _mediator;

    public SubjectController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    public Task<HandleResult<Guid>> AddNewSubject(AddNewSubjectCommand command) => _mediator.Send(command);
}