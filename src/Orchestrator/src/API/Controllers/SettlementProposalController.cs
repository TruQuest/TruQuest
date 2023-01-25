using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Settlement.Commands.CreateNewSettlementProposalDraft;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using Application.Settlement.Queries.GetSettlementProposals;

using API.Controllers.Filters;

namespace API.Controllers;

[Route("proposals")]
public class SettlementProposalController : ControllerBase
{
    private readonly ISender _mediator;

    public SettlementProposalController(ISender mediator)
    {
        _mediator = mediator;
    }

    [DisableFormValueModelBinding]
    [RequestSizeLimit(10 * 1024 * 1024)] // @@TODO: Config.
    [HttpPost("draft")]
    public Task<HandleResult<Guid>> CreateNewSettlementProposalDraft() =>
        _mediator.Send(new CreateNewSettlementProposalDraftCommand
        {
            Request = HttpContext.Request
        });

    [HttpGet("/things/{thingId}/settlement-proposals")]
    public Task<HandleResult<GetSettlementProposalsResultVm>> GetAllFor(string thingId) =>
        _mediator.Send(new GetSettlementProposalsQuery
        {
            ThingId = Guid.Parse(thingId)
        });

    [HttpPost("submit")]
    public Task<HandleResult<SubmitNewSettlementProposalResultVm>> SubmitNewSettlementProposal(
        [FromBody] SubmitNewSettlementProposalCommand command
    ) => _mediator.Send(command);
}
