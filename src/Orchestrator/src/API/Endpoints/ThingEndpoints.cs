using Application.Thing.Queries.GetVerifierLotteryParticipants;
using Application.Thing.Commands.CreateNewThingDraft;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Queries.GetThing;
using Application.Thing.Queries.GetVotes;
using Application.Thing.Commands.CastAcceptancePollVote;
using Application.Thing.Queries.GetSettlementProposalsList;
using Application.Thing.Commands.Watch;
using Infrastructure;

namespace API.Endpoints;

public static class ThingEndpoints
{
    public static RouteGroupBuilder MapThingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/things");

        group.MapPost(
            "/draft",
            (HttpRequest request, SenderWrapper sender, HttpContext context) => sender.Send(
                new CreateNewThingDraftCommand { Request = request },
                serviceProvider: context.RequestServices
            )
        );

        group.MapPost(
            "/submit",
            (SubmitNewThingCommand command, SenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapGet(
            "/{thingId}",
            ([AsParameters] GetThingQuery query, SenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapGet(
            "/{thingId}/lottery-participants",
            (
                [AsParameters] GetVerifierLotteryParticipantsQuery query,
                SenderWrapper sender,
                HttpContext context
            ) => sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapGet(
            "/{thingId}/votes",
            ([AsParameters] GetVotesQuery query, SenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/{thingId}/vote",
            (
                Guid thingId,
                CastAcceptancePollVoteCommand command,
                SenderWrapper sender,
                HttpContext context
            ) =>
            {
                command.Input.ThingId = thingId;
                return sender.Send(command, serviceProvider: context.RequestServices);
            }
        );

        group.MapGet(
            "/{thingId}/settlement-proposals",
            (
                [AsParameters] GetSettlementProposalsListQuery query,
                SenderWrapper sender,
                HttpContext context
            ) => sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/watch",
            (WatchCommand command, SenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        return group;
    }
}
