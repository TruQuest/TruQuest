namespace Domain.Errors;

public class SettlementError : HandleError
{
    public SettlementError(string message) : base(type: "Settlement")
    {
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}