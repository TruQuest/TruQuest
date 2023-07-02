using MediatR;

using Application.General.Queries.GetTags;

namespace API.Endpoints;

public static class GeneralEndpoints
{
    public static List<RouteHandlerBuilder> MapGeneralEndpoints(this WebApplication app)
    {
        return new()
        {
            app.MapGet("/tags", (ISender mediator) => mediator.Send(new GetTagsQuery()))
        };
    }
}