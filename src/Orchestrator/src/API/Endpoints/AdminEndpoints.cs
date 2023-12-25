using GoThataway;

using Application.Admin.Queries.GetContractsStates;
using Application.Admin.Commands.ToggleWithdrawals;
using Application.Admin.Commands.ToggleStopTheWorld;
using Application.Admin.Commands.EditWhitelist;
using WhitelistAction = Application.Admin.Commands.EditWhitelist.IM.ActionIm;
using Application.Admin.Commands.GiveOrRemoveRestrictedAccess;
using AccessAction = Application.Admin.Commands.GiveOrRemoveRestrictedAccess.ActionIm;
using Application.Admin.Queries.GetUserByEmail;
using Application.Admin.Commands.FundWithEthAndTru;

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

        group.MapPost(
            "/whitelist/add",
            (EditWhitelistCommand command, Thataway thataway) =>
            {
                command.Action = WhitelistAction.Add;
                return thataway.Send(command);
            }
        );

        group.MapPost(
            "/whitelist/remove",
            (EditWhitelistCommand command, Thataway thataway) =>
            {
                command.Action = WhitelistAction.Remove;
                return thataway.Send(command);
            }
        );

        group.MapGet(
            "/users/{email}",
            ([AsParameters] GetUserByEmailQuery query, Thataway thataway) => thataway.Send(query)
        );

        group.MapPost(
            "/access/give",
            (GiveOrRemoveRestrictedAccessCommand command, Thataway thataway) =>
            {
                command.Action = AccessAction.Give;
                return thataway.Send(command);
            }
        );

        group.MapPost(
            "/access/remove",
            (GiveOrRemoveRestrictedAccessCommand command, Thataway thataway) =>
            {
                command.Action = AccessAction.Remove;
                return thataway.Send(command);
            }
        );

        group.MapPost(
            "/users/{address}/fund",
            (string address, FundWithEthAndTruCommand command, Thataway thataway) =>
            {
                command.WalletAddress = address;
                return thataway.Send(command);
            }
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
