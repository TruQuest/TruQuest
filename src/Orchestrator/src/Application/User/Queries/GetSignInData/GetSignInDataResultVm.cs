namespace Application.User.Queries.GetSignInData;

public class GetSignInDataResultVm
{
    public required string Timestamp { get; init; }
    public required string Signature { get; init; }
}