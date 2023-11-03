using Application;
using Application.Thing.Queries.GetVerifierLotteryParticipants;
using Application.Thing.Commands.CreateNewThingDraft;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Queries.GetThing;
using Application.Thing.Queries.GetVotes;
using Application.Thing.Commands.CastValidationPollVote;
using Application.Thing.Queries.GetSettlementProposalsList;
using Application.Thing.Commands.Watch;

namespace API.Endpoints;

public static class ThingEndpoints
{
    public static RouteGroupBuilder MapThingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/things");

        group.MapPost(
            "/draft",
            (HttpRequest request, SenderWrapper sender) =>
                sender.Send(new CreateNewThingDraftCommand(request))
        );

        group.MapPost(
            "/submit",
            (SubmitNewThingCommand command, SenderWrapper sender) => sender.Send(command)
        );

        group.MapGet(
            "/{thingId}",
            ([AsParameters] GetThingQuery query, SenderWrapper sender) => sender.Send(query)
        );

        group.MapGet(
            "/{thingId}/lottery-participants",
            ([AsParameters] GetVerifierLotteryParticipantsQuery query, SenderWrapper sender) =>
                sender.Send(query)
        );

        group.MapGet(
            "/{thingId}/votes",
            ([AsParameters] GetVotesQuery query, SenderWrapper sender) =>
                sender.Send(query)
        );

        group.MapPost(
            "/{thingId}/vote",
            (
                Guid thingId,
                CastValidationPollVoteCommand command,
                SenderWrapper sender
            ) =>
            {
                command.Input.ThingId = thingId;
                return sender.Send(command);
            }
        );

        group.MapGet(
            "/{thingId}/settlement-proposals",
            ([AsParameters] GetSettlementProposalsListQuery query, SenderWrapper sender) =>
                sender.Send(query)
        );

        group.MapPost(
            "/watch",
            (WatchCommand command, SenderWrapper sender) => sender.Send(command)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
