namespace Infrastructure.Ethereum.Errors;

public class NonCustomRevertError : BaseError
{
    public string Message { get; }

    public NonCustomRevertError(string message)
    {
        Message = message;
    }

    public override string ToString() => Message;
}
