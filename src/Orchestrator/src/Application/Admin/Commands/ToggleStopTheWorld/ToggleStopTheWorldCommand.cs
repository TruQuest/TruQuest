using GoThataway;

using Domain.Results;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Admin.Commands.ToggleStopTheWorld;

[RequireAuthorization(Policy = "AdminOnly")]
public class ToggleStopTheWorldCommand : IRequest<HandleResult<string>>
{
    public required bool Value { get; init; }
}

public class ToggleStopTheWorldCommandHandler : IRequestHandler<ToggleStopTheWorldCommand, HandleResult<string>>
{
    private readonly IContractCaller _contractCaller;

    public ToggleStopTheWorldCommandHandler(IContractCaller contractCaller)
    {
        _contractCaller = contractCaller;
    }

    public async Task<HandleResult<string>> Handle(ToggleStopTheWorldCommand command, CancellationToken ct)
    {
        var txnHash = await _contractCaller.ToggleStopTheWorld(command.Value);
        if (txnHash == null)
        {
            return new()
            {
                Error = new HandleError("Error trying to set s_stopTheWorld to " + command.Value)
            };
        }

        return new()
        {
            Data = txnHash
        };
    }
}
