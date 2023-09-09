using Application;
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
                (SenderWrapper sender, HttpContext context) =>
                    sender.Send(new GetTagsQuery(), serviceProvider: context.RequestServices)
            )
        };

        foreach (var endpoint in endpoints) endpoint.AddEndpointFilter(Filters.ConvertHandleResult);

        return endpoints;
    }
}
