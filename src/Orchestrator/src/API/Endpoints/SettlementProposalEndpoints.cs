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
            (HttpRequest request, SenderWrapper sender, HttpContext context) => sender.Send(
                new CreateNewSettlementProposalDraftCommand
                {
                    Request = request
                },
                serviceProvider: context.RequestServices
            )
        );

        group.MapGet(
            "/{proposalId}",
            ([AsParameters] GetSettlementProposalQuery query, SenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/submit",
            (SubmitNewSettlementProposalCommand command, SenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        app
            .MapGet(
                "/things/{thingId}/proposals/{proposalId}/lottery-participants",
                (
                    [AsParameters] GetVerifierLotteryParticipantsQuery query,
                    SenderWrapper sender,
                    HttpContext context
                ) => sender.Send(query, serviceProvider: context.RequestServices)
            )
            .AddEndpointFilter(Filters.ConvertHandleResult);

        group.MapGet(
            "/{proposalId}/votes",
            ([AsParameters] GetVotesQuery query, SenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/{proposalId}/vote",
            (
                Guid proposalId,
                CastAssessmentPollVoteCommand command,
                SenderWrapper sender,
                HttpContext context
            ) =>
            {
                command.Input.SettlementProposalId = proposalId;
                return sender.Send(command, serviceProvider: context.RequestServices);
            }
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
