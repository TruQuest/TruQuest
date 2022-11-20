namespace Domain.Errors;

public class SubjectError : HandleError
{
    public SubjectError(string message) : base(type: "Subject")
    {
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}