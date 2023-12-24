using GoThataway;

using Domain.Results;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Admin.Commands.ToggleWithdrawals;

[RequireAuthorization(Policy = "AdminOnly")]
public class ToggleWithdrawalsCommand : IRequest<HandleResult<string>>
{
    public required bool Value { get; init; }
}

public class ToggleWithdrawalsCommandHandler : IRequestHandler<ToggleWithdrawalsCommand, HandleResult<string>>
{
    private readonly IContractCaller _contractCaller;

    public ToggleWithdrawalsCommandHandler(IContractCaller contractCaller)
    {
        _contractCaller = contractCaller;
    }

    public async Task<HandleResult<string>> Handle(ToggleWithdrawalsCommand command, CancellationToken ct)
    {
        var txnHash = await _contractCaller.ToggleWithdrawals(command.Value);
        if (txnHash == null)
        {
            return new()
            {
                Error = new HandleError("Error trying to set s_withdrawalsEnabled to " + command.Value)
            };
        }

        return new()
        {
            Data = txnHash
        };
    }
}
