using GoThataway;

using Application.Admin.Queries.GetContractsStates;
using Application.Admin.Commands.ToggleWithdrawals;
using Application.Admin.Commands.ToggleStopTheWorld;

namespace API.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin");

        group.MapGet(
            "/contracts-states",
            (Thataway thataway) => thataway.Send(new GetContractsStatesQuery())
        );

        group.MapPost(
            "/withdrawals/{value}",
            ([AsParameters] ToggleWithdrawalsCommand command, Thataway thataway) => thataway.Send(command)
        );

        group.MapPost(
            "/stop-the-world/{value}",
            ([AsParameters] ToggleStopTheWorldCommand command, Thataway thataway) => thataway.Send(command)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
