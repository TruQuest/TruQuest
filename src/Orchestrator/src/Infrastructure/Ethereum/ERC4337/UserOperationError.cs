namespace Infrastructure.Ethereum.ERC4337;

public class UserOperationError
{
    public int? Code { get; }
    public string Message { get; }

    public bool IsPreVerificationGasTooLow =>
      Code == -32602 && Message.StartsWith("preVerificationGas too low");

    public bool IsOverVerificationGasLimit =>
        Code == -32500 &&
        Message == "account validation failed: AA40 over verificationGasLimit";

    public UserOperationError(string? reason)
    {
        Code = null;
        Message = reason ?? "Unspecified reason";
    }

    public UserOperationError(int code, string message)
    {
        Code = code;
        Message = message;
    }
}
