using Microsoft.AspNetCore.Mvc;

using MediatR;

using Domain.Results;
using Application.Settlement.Commands.CreateNewSettlementProposalDraft;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using Application.Settlement.Queries.GetSettlementProposals;
using Application.Settlement.Queries.GetSettlementProposal;
using Application.Settlement.Queries.GetVerifierLotteryParticipants;
using Application.Settlement.Commands.CastAssessmentPollVote;
using Application.Settlement.Queries.GetVerifiers;

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

    [HttpGet("{id}")]
    public Task<HandleResult<GetSettlementProposalResultVm>> Get(string id) =>
        _mediator.Send(new GetSettlementProposalQuery { ProposalId = Guid.Parse(id) });

    [HttpPost("submit")]
    public Task<HandleResult<SubmitNewSettlementProposalResultVm>> SubmitNewSettlementProposal(
        [FromBody] SubmitNewSettlementProposalCommand command
    ) => _mediator.Send(command);

    [HttpGet("{id}/lottery-participants")]
    public Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> GetVerifierLotteryParticipants(string id) =>
        _mediator.Send(new GetVerifierLotteryParticipantsQuery
        {
            ProposalId = Guid.Parse(id)
        });

    [HttpPost("{id}/vote")]
    public Task<HandleResult<string>> CastAssessmentPollVote(
        [FromRoute] string id, [FromBody] CastAssessmentPollVoteCommand command
    )
    {
        command.Input.SettlementProposalId = Guid.Parse(id);
        return _mediator.Send(command);
    }

    [HttpGet("{id}/verifiers")]
    public Task<HandleResult<GetVerifiersResultVm>> GetVerifiers(string id) =>
        _mediator.Send(new GetVerifiersQuery { ProposalId = Guid.Parse(id) });
}
