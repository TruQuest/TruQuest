using Application.User.Commands.MarkNotificationsAsRead;
using Application.User.Commands.SignInWithEthereum;
using Application.User.Queries.GetNonceForSiwe;
using Application.User.Commands.SignUp;
using Application.User.Commands.GenerateConfirmationCodeAndAttestationOptions;
using Application.User.Commands.GenerateAssertionOptions;
using Application.User.Commands.VerifyAssertionAndGetKeyShare;
using Application.User.Commands.GenerateAssertionOptionsForSignIn;
using Application.User.Commands.VerifyAssertionAndSignIn;
using Application.Common.Interfaces;

namespace API.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/user");

        group.MapGet(
            "/siwe/{address}",
            ([AsParameters] GetNonceForSiweQuery query, ISenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/siwe",
            (SignInWithEthereumCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/watch-list",
            (MarkNotificationsAsReadCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/generate-code-and-attestation-options",
            (GenerateConfirmationCodeAndAttestationOptionsCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/sign-up",
            (SignUpCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/generate-assertion-options",
            (GenerateAssertionOptionsCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/verify-assertion-and-get-key-share",
            (VerifyAssertionAndGetKeyShareCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/generate-assertion-options-for-sign-in",
            (GenerateAssertionOptionsForSignInCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.MapPost(
            "/verify-assertion-and-sign-in",
            (VerifyAssertionAndSignInCommand command, ISenderWrapper sender, HttpContext context) =>
                sender.Send(command, serviceProvider: context.RequestServices)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
