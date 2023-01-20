namespace Application.User.Commands.SignIn;

public class SignInResultVm
{
    public required string Username { get; init; }
    public required string Token { get; init; }
}