using MediatR;

using Application.Thing.Queries.GetVerifierLotteryParticipants;
using Application.Thing.Commands.CreateNewThingDraft;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Queries.GetThing;
using Application.Thing.Queries.GetVerifiers;
using Application.Thing.Commands.CastAcceptancePollVote;
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
            (HttpRequest request, ISender mediator) => mediator.Send(
                new CreateNewThingDraftCommand { Request = request }
            )
        );

        group.MapPost(
            "/submit",
            (SubmitNewThingCommand command, ISender mediator) =>
                mediator.Send(command)
        );

        group.MapGet(
            "/{thingId}",
            ([AsParameters] GetThingQuery query, ISender mediator) =>
                mediator.Send(query)
        );

        group.MapGet(
            "/{thingId}/lottery-participants",
            (
                [AsParameters] GetVerifierLotteryParticipantsQuery query,
                ISender mediator
            ) => mediator.Send(query)
        );

        group.MapGet(
            "/{thingId}/verifiers",
            ([AsParameters] GetVerifiersQuery query, ISender mediator) =>
                mediator.Send(query)
        );

        group.MapPost(
            "/{thingId}/vote",
            (
                Guid thingId,
                CastAcceptancePollVoteCommand command,
                ISender mediator
            ) =>
            {
                command.Input.ThingId = thingId;
                return mediator.Send(command);
            }
        );

        group.MapGet(
            "/{thingId}/settlement-proposals",
            (
                [AsParameters] GetSettlementProposalsListQuery query,
                ISender mediator
            ) => mediator.Send(query)
        );

        group.MapPost(
            "/watch",
            (WatchCommand command, ISender mediator) => mediator.Send(command)
        );

        return group;
    }
}