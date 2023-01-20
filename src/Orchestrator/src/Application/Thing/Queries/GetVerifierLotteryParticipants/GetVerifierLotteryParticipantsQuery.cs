using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Thing.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsQuery : IRequest<HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class GetVerifierLotteryParticipantsQueryHandler :
    IRequestHandler<GetVerifierLotteryParticipantsQuery, HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    private readonly IThingQueryable _thingQueryable;

    public GetVerifierLotteryParticipantsQueryHandler(IThingQueryable thingQueryable)
    {
        _thingQueryable = thingQueryable;
    }

    public async Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> Handle(GetVerifierLotteryParticipantsQuery query, CancellationToken ct)
    {
        var entries = await _thingQueryable.GetVerifierLotteryParticipants(query.ThingId);
        return new()
        {
            Data = new()
            {
                ThingId = query.ThingId,
                Entries = entries
            }
        };
    }
}