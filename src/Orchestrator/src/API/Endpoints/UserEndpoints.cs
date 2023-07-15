using MediatR;

using Application.User.Commands.MarkNotificationsAsRead;
using Application.User.Commands.SignInWithEthereum;
using Application.User.Queries.GetNonceForSiwe;
using Application.User.Commands.AddEmail;
using Application.User.Commands.ConfirmEmail;

namespace API.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/user");

        group.MapGet(
            "/siwe/{address}",
            ([AsParameters] GetNonceForSiweQuery query, ISender mediator) =>
                mediator.Send(query)
        );

        group.MapPost(
            "/siwe",
            (SignInWithEthereumCommand command, ISender mediator) =>
                mediator.Send(command)
        );

        group.MapPost(
            "/email",
            (AddEmailCommand command, ISender mediator) => mediator.Send(command)
        );

        group.MapPost(
            "/email/confirm",
            (ConfirmEmailCommand command, ISender mediator) => mediator.Send(command)
        );

        group.MapPost(
            "/watch-list",
            (MarkNotificationsAsReadCommand command, ISender mediator) =>
                mediator.Send(command)
        );

        return group;
    }
}
