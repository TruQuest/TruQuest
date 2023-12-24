namespace Application.Admin.Queries.GetContractsStates.QM;

public class UserBalanceQm
{
    public required string Address { get; init; }
    public required string HexBalance { get; init; }
    public required string HexStakedBalance { get; init; }
}
