using System.Diagnostics;

using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Thing.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsQuery : IRequest<HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class Validator : AbstractValidator<GetVerifierLotteryParticipantsQuery>
{
    public Validator()
    {
        RuleFor(q => q.ThingId).NotEmpty();
    }
}

internal class GetVerifierLotteryParticipantsQueryHandler :
    IRequestHandler<GetVerifierLotteryParticipantsQuery, HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    private readonly IThingQueryable _thingQueryable;
    private readonly ISigner _signer;

    public GetVerifierLotteryParticipantsQueryHandler(
        IThingQueryable thingQueryable,
        ISigner signer
    )
    {
        _thingQueryable = thingQueryable;
        _signer = signer;
    }

    public async Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> Handle(
        GetVerifierLotteryParticipantsQuery query, CancellationToken ct
    )
    {
        var participants = await _thingQueryable.GetVerifierLotteryParticipants(query.ThingId);
        var latestParticipant = participants.FirstOrDefault();
        if (latestParticipant != null && _signer.CheckIsOrchestrator(latestParticipant.UserId))
        {
            // means the lottery was closed with success
            Debug.Assert(latestParticipant.Nonce != null && latestParticipant.UserData == null);
            latestParticipant.MarkAsOrchestrator();

            var verifierIds = await _thingQueryable.GetVerifiers(query.ThingId);
            Debug.Assert(verifierIds.Any());
            foreach (var verifierId in verifierIds)
            {
                var entry = participants.Single(e => e.UserId == verifierId);
                entry.MarkAsWinner();
            }
        }
        else
        {
            // @@NOTE: There is a period when the lottery is closed (and therefore the nonces are set)
            // but not yet finalized (verifiers are not yet added to the db, etc.). So we just clear
            // nonces here since it would be confusing to already show nonces but not winners. 
            var entry = participants.FirstOrDefault();
            if (entry?.Nonce != null)
            {
                // if one entry's nonce is set then all entries' nonces are set since we set them in a txn
                foreach (var e in participants) e.ClearSensitiveData();
            }
        }

        participants = participants
            .OrderBy(e => e.SortKey)
            .ThenByDescending(e => e.BlockNumber);

        return new()
        {
            Data = new()
            {
                ThingId = query.ThingId,
                Participants = participants
            }
        };
    }
}
