using Application;
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
            ([AsParameters] GetNonceForSiweQuery query, SenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/siwe",
            (SignInWithEthereumCommand command, SenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/email",
            (AddEmailCommand command, SenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/email/confirm",
            (ConfirmEmailCommand command, SenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/watch-list",
            (MarkNotificationsAsReadCommand command, SenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
