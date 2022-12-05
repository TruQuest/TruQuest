namespace Domain.Errors;

public class VoteError : HandleError
{
    public VoteError(string message) : base(type: "Vote")
    {
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}