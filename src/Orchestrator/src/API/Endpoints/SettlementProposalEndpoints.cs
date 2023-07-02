using MediatR;

using Application.Settlement.Commands.CastAssessmentPollVote;
using Application.Settlement.Commands.CreateNewSettlementProposalDraft;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using Application.Settlement.Queries.GetSettlementProposal;
using Application.Settlement.Queries.GetVerifierLotteryParticipants;
using Application.Settlement.Queries.GetVerifiers;

namespace API.Endpoints;

public static class SettlementProposalEndpoints
{
    public static RouteGroupBuilder MapSettlementProposalEndpoints(
        this WebApplication app
    )
    {
        var group = app.MapGroup("/proposals");

        group.MapPost(
            "/draft",
            (HttpRequest request, ISender mediator) => mediator.Send(
                new CreateNewSettlementProposalDraftCommand
                {
                    Request = request
                }
            )
        );

        group.MapGet(
            "/{proposalId}",
            ([AsParameters] GetSettlementProposalQuery query, ISender mediator) =>
                mediator.Send(query)
        );

        group.MapPost(
            "/submit",
            (SubmitNewSettlementProposalCommand command, ISender mediator) =>
                mediator.Send(command)
        );

        group.MapGet(
            "/{proposalId}/lottery-participants",
            (
                [AsParameters] GetVerifierLotteryParticipantsQuery query,
                ISender mediator
            ) => mediator.Send(query)
        );

        group.MapGet(
            "/{proposalId}/verifiers",
            ([AsParameters] GetVerifiersQuery query, ISender mediator) =>
                mediator.Send(query)
        );

        group.MapPost(
            "/{proposalId}/vote",
            (
                Guid proposalId,
                CastAssessmentPollVoteCommand command,
                ISender mediator
            ) =>
            {
                command.Input.SettlementProposalId = proposalId;
                return mediator.Send(command);
            }
        );

        return group;
    }
}