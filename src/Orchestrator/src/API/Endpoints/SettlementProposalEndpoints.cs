using GoThataway;

using Application.Settlement.Commands.CastAssessmentPollVote;
using Application.Settlement.Commands.CreateNewSettlementProposalDraft;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using Application.Settlement.Queries.GetSettlementProposal;
using Application.Settlement.Queries.GetVerifierLotteryParticipants;
using Application.Settlement.Queries.GetVotes;

namespace API.Endpoints;

public static class SettlementProposalEndpoints
{
    public static RouteGroupBuilder MapSettlementProposalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/proposals");

        group.MapPost(
            "/draft",
            (HttpRequest request, Thataway thataway) =>
                thataway.Send(new CreateNewSettlementProposalDraftCommand(request))
        );

        group.MapGet(
            "/{proposalId}",
            ([AsParameters] GetSettlementProposalQuery query, Thataway thataway) =>
                thataway.Send(query)
        );

        group.MapPost(
            "/submit",
            (SubmitNewSettlementProposalCommand command, Thataway thataway) =>
                thataway.Send(command)
        );

        app
            .MapGet(
                "/things/{thingId}/proposals/{proposalId}/lottery-participants",
                ([AsParameters] GetVerifierLotteryParticipantsQuery query, Thataway thataway) =>
                    thataway.Send(query)
            )
            .AddEndpointFilter(Filters.ConvertHandleResult);

        group.MapGet(
            "/{proposalId}/votes",
            ([AsParameters] GetVotesQuery query, Thataway thataway) =>
                thataway.Send(query)
        );

        group.MapPost(
            "/{proposalId}/vote",
            (
                Guid proposalId,
                CastAssessmentPollVoteCommand command,
                Thataway thataway
            ) =>
            {
                command.Input.SettlementProposalId = proposalId;
                return thataway.Send(command);
            }
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
