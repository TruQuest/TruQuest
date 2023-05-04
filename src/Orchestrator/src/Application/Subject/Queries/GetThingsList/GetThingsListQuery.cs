using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Subject.Queries.GetThingsList;

public class GetThingsListQuery : IRequest<HandleResult<GetThingsListResultVm>>
{
    public required Guid SubjectId { get; init; }
}

internal class GetThingsListQueryHandler : IRequestHandler<GetThingsListQuery, HandleResult<GetThingsListResultVm>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IThingQueryable _thingQueryable;

    public GetThingsListQueryHandler(
        ICurrentPrincipal currentPrincipal,
        IThingQueryable thingQueryable
    )
    {
        _currentPrincipal = currentPrincipal;
        _thingQueryable = thingQueryable;
    }

    public async Task<HandleResult<GetThingsListResultVm>> Handle(GetThingsListQuery query, CancellationToken ct)
    {
        var things = await _thingQueryable.GetForSubject(query.SubjectId, _currentPrincipal.Id);
        return new()
        {
            Data = new()
            {
                SubjectId = query.SubjectId,
                Things = things
            }
        };
    }
}