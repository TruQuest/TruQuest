using Application;
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
            (HttpRequest request, SenderWrapper sender) =>
                sender.Send(new CreateNewSettlementProposalDraftCommand(request))
        );

        group.MapGet(
            "/{proposalId}",
            ([AsParameters] GetSettlementProposalQuery query, SenderWrapper sender) =>
                sender.Send(query)
        );

        group.MapPost(
            "/submit",
            (SubmitNewSettlementProposalCommand command, SenderWrapper sender) =>
                sender.Send(command)
        );

        app
            .MapGet(
                "/things/{thingId}/proposals/{proposalId}/lottery-participants",
                ([AsParameters] GetVerifierLotteryParticipantsQuery query, SenderWrapper sender) =>
                    sender.Send(query)
            )
            .AddEndpointFilter(Filters.ConvertHandleResult);

        group.MapGet(
            "/{proposalId}/votes",
            ([AsParameters] GetVotesQuery query, SenderWrapper sender) =>
                sender.Send(query)
        );

        group.MapPost(
            "/{proposalId}/vote",
            (
                Guid proposalId,
                CastAssessmentPollVoteCommand command,
                SenderWrapper sender
            ) =>
            {
                command.Input.SettlementProposalId = proposalId;
                return sender.Send(command);
            }
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
