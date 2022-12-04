namespace Domain.Errors;

public class UserError : HandleError
{
    public UserError(Dictionary<string, string[]> errors) : base(type: "User")
    {
        Errors = errors;
    }

    public UserError(string message) : base(type: "User")
    {
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}