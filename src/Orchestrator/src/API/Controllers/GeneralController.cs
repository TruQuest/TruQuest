using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Common.Models.QM;
using Application.General.Queries.GetTags;

namespace API.Controllers;

public class GeneralController : ControllerBase
{
    private readonly ISender _mediator;

    public GeneralController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("tags")]
    public Task<HandleResult<IEnumerable<TagQm>>> GetTags() => _mediator.Send(new GetTagsQuery());
}