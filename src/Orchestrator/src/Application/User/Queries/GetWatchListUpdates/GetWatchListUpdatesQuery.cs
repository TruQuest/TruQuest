using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.User.Queries.GetWatchListUpdates;

public class GetWatchListUpdatesQuery : IRequest<HandleResult<IEnumerable<WatchedItemUpdateQm>>>
{
    public required string UserId { get; init; }
}

internal class GetWatchListUpdatesQueryHandler :
    IRequestHandler<GetWatchListUpdatesQuery, HandleResult<IEnumerable<WatchedItemUpdateQm>>>
{
    private readonly IWatchListQueryable _watchListQueryable;

    public GetWatchListUpdatesQueryHandler(IWatchListQueryable watchListQueryable)
    {
        _watchListQueryable = watchListQueryable;
    }

    public async Task<HandleResult<IEnumerable<WatchedItemUpdateQm>>> Handle(
        GetWatchListUpdatesQuery query, CancellationToken ct
    )
    {
        var updates = await _watchListQueryable.GetLatestUpdatesFor(query.UserId);
        return new()
        {
            Data = updates
        };
    }
}