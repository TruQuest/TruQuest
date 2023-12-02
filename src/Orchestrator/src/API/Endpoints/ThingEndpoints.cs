using GoThataway;

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
        var group = app.MapGroup("/api/things");

        group.MapPost(
            "/draft",
            (HttpRequest request, Thataway thataway) =>
                thataway.Send(new CreateNewThingDraftCommand(request))
        );

        group.MapPost(
            "/submit",
            (SubmitNewThingCommand command, Thataway thataway) => thataway.Send(command)
        );

        group.MapGet(
            "/{thingId}",
            ([AsParameters] GetThingQuery query, Thataway thataway) => thataway.Send(query)
        );

        group.MapGet(
            "/{thingId}/lottery-participants",
            ([AsParameters] GetVerifierLotteryParticipantsQuery query, Thataway thataway) =>
                thataway.Send(query)
        );

        group.MapGet(
            "/{thingId}/votes",
            ([AsParameters] GetVotesQuery query, Thataway thataway) =>
                thataway.Send(query)
        );

        group.MapPost(
            "/{thingId}/vote",
            (
                Guid thingId,
                CastValidationPollVoteCommand command,
                Thataway thataway
            ) =>
            {
                command.Input.ThingId = thingId;
                return thataway.Send(command);
            }
        );

        group.MapGet(
            "/{thingId}/settlement-proposals",
            ([AsParameters] GetSettlementProposalsListQuery query, Thataway thataway) =>
                thataway.Send(query)
        );

        group.MapPost(
            "/watch",
            (WatchCommand command, Thataway thataway) => thataway.Send(command)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
