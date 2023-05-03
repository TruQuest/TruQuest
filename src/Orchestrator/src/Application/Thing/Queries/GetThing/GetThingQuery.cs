using MediatR;

using Domain.Results;
using Domain.Errors;

using Application.Common.Interfaces;
using Application.Common.Models.QM;

namespace Application.Thing.Queries.GetThing;

public class GetThingQuery : IRequest<HandleResult<GetThingResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class GetThingQueryHandler : IRequestHandler<GetThingQuery, HandleResult<GetThingResultVm>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IThingQueryable _thingQueryable;
    private readonly ISigner _signer;

    public GetThingQueryHandler(
        ICurrentPrincipal currentPrincipal,
        IThingQueryable thingQueryable,
        ISigner signer
    )
    {
        _currentPrincipal = currentPrincipal;
        _thingQueryable = thingQueryable;
        _signer = signer;
    }

    public async Task<HandleResult<GetThingResultVm>> Handle(GetThingQuery query, CancellationToken ct)
    {
        var thing = await _thingQueryable.GetById(query.ThingId);
        if (thing == null)
        {
            return new()
            {
                Error = new ThingError("Not Found")
            };
        }
        if (thing.State == ThingStateQm.Draft && _currentPrincipal.Id != thing.SubmitterId)
        {
            return new()
            {
                Error = new ThingError("Invalid request")
            };
        }

        string? signature = null;
        if (
            _currentPrincipal.Id != null &&
            thing.SubmitterId == _currentPrincipal.Id &&
            thing.State == ThingStateQm.AwaitingFunding
        )
        {
            signature = _signer.SignThing(thing.Id);
        }

        return new()
        {
            Data = new()
            {
                Thing = thing,
                Signature = signature
            }
        };
    }
}