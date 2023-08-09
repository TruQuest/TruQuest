using Application.General.Queries.GetTags;
using Infrastructure;

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
