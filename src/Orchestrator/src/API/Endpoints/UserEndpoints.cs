using GoThataway;

using Application.User.Commands.MarkNotificationsAsRead;
using Application.User.Commands.SignInWithEthereum;
using Application.User.Queries.GetNonceForSiwe;
using Application.User.Commands.SignUp;
using Application.User.Commands.GenerateConfirmationCodeAndAttestationOptions;
using Application.User.Commands.GenerateAssertionOptions;
using Application.User.Commands.VerifyAssertionAndGetKeyShare;
using Application.User.Commands.GenerateAssertionOptionsForSignIn;
using Application.User.Commands.VerifyAssertionAndSignIn;

namespace API.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/user");

        group.MapGet(
            "/siwe/{address}",
            ([AsParameters] GetNonceForSiweQuery query, Thataway thataway) =>
                thataway.Send(query)
        );

        group.MapPost(
            "/siwe",
            (SignInWithEthereumCommand command, Thataway thataway) =>
                thataway.Send(command)
        );

        group.MapPost(
            "/watch-list",
            (MarkNotificationsAsReadCommand command, Thataway thataway) =>
                thataway.Send(command)
        );

        group.MapPost(
            "/generate-code-and-attestation-options",
            (GenerateConfirmationCodeAndAttestationOptionsCommand command, Thataway thataway) =>
                thataway.Send(command)
        );

        group.MapPost(
            "/sign-up",
            (SignUpCommand command, Thataway thataway) => thataway.Send(command)
        );

        group.MapPost(
            "/generate-assertion-options",
            (GenerateAssertionOptionsCommand command, Thataway thataway) =>
                thataway.Send(command)
        );

        group.MapPost(
            "/verify-assertion-and-get-key-share",
            (VerifyAssertionAndGetKeyShareCommand command, Thataway thataway) =>
                thataway.Send(command)
        );

        group.MapPost(
            "/generate-assertion-options-for-sign-in",
            (GenerateAssertionOptionsForSignInCommand command, Thataway thataway) =>
                thataway.Send(command)
        );

        group.MapPost(
            "/verify-assertion-and-sign-in",
            (VerifyAssertionAndSignInCommand command, Thataway thataway) =>
                thataway.Send(command)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
