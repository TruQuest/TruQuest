using Application;
using Application.General.Queries.GetTags;

namespace API.Endpoints;

public static class GeneralEndpoints
{
    public static List<RouteHandlerBuilder> MapGeneralEndpoints(this WebApplication app)
    {
        return new()
        {
            app.MapGet("/tags", (SenderWrapper sender, HttpContext context) =>
                sender.Send(new GetTagsQuery(), serviceProvider: context.RequestServices))
        };
    }
}
