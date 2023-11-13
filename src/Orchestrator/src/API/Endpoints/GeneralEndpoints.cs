using GoThataway;

using Application.General.Queries.GetTags;

namespace API.Endpoints;

public static class GeneralEndpoints
{
    public static List<RouteHandlerBuilder> MapGeneralEndpoints(this WebApplication app)
    {
        var endpoints = new List<RouteHandlerBuilder>()
        {
            app.MapGet(
                "/tags",
                (Thataway thataway) => thataway.Send(new GetTagsQuery())
            )
        };

        foreach (var endpoint in endpoints) endpoint.AddEndpointFilter(Filters.ConvertHandleResult);

        return endpoints;
    }
}
