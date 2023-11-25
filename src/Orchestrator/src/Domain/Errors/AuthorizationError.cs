namespace Domain.Errors;

public class AuthorizationError : HandleError
{
    public AuthorizationError(string message) : base(message) { }
}
