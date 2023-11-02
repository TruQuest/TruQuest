using Application.Thing.Queries.GetVerifierLotteryParticipants;
using Application.Thing.Commands.CreateNewThingDraft;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Queries.GetThing;
using Application.Thing.Queries.GetVotes;
using Application.Thing.Commands.CastValidationPollVote;
using Application.Thing.Queries.GetSettlementProposalsList;
using Application.Thing.Commands.Watch;
using Application.Common.Interfaces;

namespace API.Endpoints;

public static class ThingEndpoints
{
    public static RouteGroupBuilder MapThingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/things");

        group.MapPost(
            "/draft",
            (HttpRequest request, ISenderWrapper sender, HttpContext context) => sender.Send(
                new CreateNewThingDraftCommand(request),
                serviceProvider: context.RequestServices
            )
        );

        group.MapPost(
            "/submit",
            (SubmitNewThingCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapGet(
            "/{thingId}",
            ([AsParameters] GetThingQuery query, ISenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapGet(
            "/{thingId}/lottery-participants",
            (
                [AsParameters] GetVerifierLotteryParticipantsQuery query,
                ISenderWrapper sender,
                HttpContext context
            ) => sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapGet(
            "/{thingId}/votes",
            ([AsParameters] GetVotesQuery query, ISenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/{thingId}/vote",
            (
                Guid thingId,
                CastValidationPollVoteCommand command,
                ISenderWrapper sender,
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
                ISenderWrapper sender,
                HttpContext context
            ) => sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/watch",
            (WatchCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
