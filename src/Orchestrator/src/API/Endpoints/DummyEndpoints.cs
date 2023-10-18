using Application;
using Application.Dummy.Commands.AddCredential;
using Application.Dummy.Commands.CreateAuthOptions;
using Application.Dummy.Commands.CreateRegOptions;
using Application.Dummy.Commands.CreateUser;
using Application.Dummy.Commands.SaveShare;
using Application.Dummy.Commands.VerifyCredential;

namespace API.Endpoints;

public static class DummyEndpoints
{
    public static List<RouteHandlerBuilder> MapDummyEndpoints(this WebApplication app)
    {
        var endpoints = new List<RouteHandlerBuilder>()
        {
            app.MapPost(
                "/dummy/create-user",
                (CreateUserCommand command, SenderWrapper sender, HttpContext context) =>
                    sender.Send(command, serviceProvider: context.RequestServices)
            ),
            app.MapPost(
                "/dummy/create-reg-options",
                (CreateRegOptionsCommand command, SenderWrapper sender, HttpContext context) =>
                    sender.Send(command, serviceProvider: context.RequestServices)
            ),
            app.MapPost(
                "/dummy/add-credential",
                (AddCredentialCommand command, SenderWrapper sender, HttpContext context) =>
                    sender.Send(command, serviceProvider: context.RequestServices)
            ),
            app.MapPost(
                "/dummy/create-auth-options",
                (CreateAuthOptionsCommand command, SenderWrapper sender, HttpContext context) =>
                    sender.Send(command, serviceProvider: context.RequestServices)
            ),
            app.MapPost(
                "/dummy/verify-credential",
                (VerifyCredentialCommand command, SenderWrapper sender, HttpContext context) =>
                    sender.Send(command, serviceProvider: context.RequestServices)
            ),
            app.MapPost(
                "/dummy/save-share",
                (SaveShareCommand command, SenderWrapper sender, HttpContext context) =>
                    sender.Send(command, serviceProvider: context.RequestServices)
            ),
        };

        foreach (var endpoint in endpoints) endpoint.AddEndpointFilter(Filters.ConvertHandleResult);

        return endpoints;
    }
}
