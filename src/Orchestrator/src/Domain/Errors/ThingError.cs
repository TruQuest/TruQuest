namespace Domain.Errors;

public class ThingError : HandleError
{
    public ThingError(string message) : base(type: "Thing")
    {
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}