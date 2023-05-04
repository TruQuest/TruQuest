using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Subject.Commands.AddNewSubject;
using Application.Subject.Queries.GetSubject;
using Application.Subject.Queries.GetThingsList;

using API.Controllers.Filters;

namespace API.Controllers;

[Route("subjects")]
public class SubjectController : ControllerBase
{
    private readonly ISender _mediator;

    public SubjectController(ISender mediator)
    {
        _mediator = mediator;
    }

    [DisableFormValueModelBinding]
    [RequestSizeLimit(10 * 1024 * 1024)] // @@TODO: Config.
    [HttpPost("add")]
    public Task<HandleResult<Guid>> AddNewSubject() => _mediator.Send(new AddNewSubjectCommand
    {
        Request = HttpContext.Request
    });

    [HttpGet("{id}")]
    public Task<HandleResult<SubjectQm>> Get(Guid id) => _mediator.Send(new GetSubjectQuery
    {
        Id = id
    });

    [HttpGet("{subjectId}/things")]
    public Task<HandleResult<GetThingsListResultVm>> GetThingsList(GetThingsListQuery query) =>
        _mediator.Send(query);
}