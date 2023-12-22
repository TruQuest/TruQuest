using GoThataway;

using Application.General.Queries.GetTags;
using Application.General.Queries.GetContractsStates;

namespace API.Endpoints;

public static class GeneralEndpoints
{
    public static List<RouteHandlerBuilder> MapGeneralEndpoints(this WebApplication app)
    {
        var endpoints = new List<RouteHandlerBuilder>()
        {
            app.MapGet(
                "/api/tags",
                (Thataway thataway) => thataway.Send(new GetTagsQuery())
            ),
            app.MapGet(
                "/api/contracts-states",
                (Thataway thataway) => thataway.Send(new GetContractsStatesQuery())
            )
        };

        foreach (var endpoint in endpoints) endpoint.AddEndpointFilter(Filters.ConvertHandleResult);

        return endpoints;
    }
}
