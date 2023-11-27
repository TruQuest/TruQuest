using GoThataway;

using Application.General.Queries.GetTags;
using Application.General.Queries.GetFrontendEnvFile;

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
            ),
            app.MapGet(
                "/assets/.env",
                async (Thataway thataway) =>
                {
                    var result = await thataway.Send(new GetFrontendEnvFileQuery());
                    return result.Data;
                }
            )
        };

        foreach (var endpoint in endpoints.SkipLast(1)) endpoint.AddEndpointFilter(Filters.ConvertHandleResult);

        return endpoints;
    }
}
