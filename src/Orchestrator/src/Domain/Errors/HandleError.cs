namespace Domain.Errors;

public class HandleError
{
    public string Message { get; }

    public HandleError(string message)
    {
        Message = message;
    }

    public override string ToString() => Message;
}
