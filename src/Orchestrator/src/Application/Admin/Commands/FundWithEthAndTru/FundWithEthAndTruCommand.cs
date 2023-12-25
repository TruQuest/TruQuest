using System.Numerics;

using GoThataway;
using FluentValidation;

using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Admin.Commands.FundWithEthAndTru;

[RequireAuthorization(Policy = "AdminOnly")]
public class FundWithEthAndTruCommand : IRequest<HandleResult<FundWithEthAndTruRvm>>
{
    public string WalletAddress { get; set; }
    public required decimal AmountInEth { get; init; }
    public required decimal AmountInTru { get; init; }
}

internal class Validator : AbstractValidator<FundWithEthAndTruCommand>
{
    public Validator(IEthereumAddressFormatter addressFormatter)
    {
        RuleFor(c => addressFormatter.IsValidEIP55EncodedAddress(c.WalletAddress));
        RuleFor(c => c.AmountInEth).GreaterThanOrEqualTo(0);
        RuleFor(c => c.AmountInTru).GreaterThanOrEqualTo(0);
    }
}

public class FundWithEthAndTruCommandHandler : IRequestHandler<FundWithEthAndTruCommand, HandleResult<FundWithEthAndTruRvm>>
{
    private readonly IContractCaller _contractCaller;

    public FundWithEthAndTruCommandHandler(IContractCaller contractCaller)
    {
        _contractCaller = contractCaller;
    }

    public async Task<HandleResult<FundWithEthAndTruRvm>> Handle(FundWithEthAndTruCommand command, CancellationToken ct)
    {
        string? fundWithEthTxnHash = null;
        if (command.AmountInEth > 0)
        {
            fundWithEthTxnHash = await _contractCaller.FundWithEth(command.WalletAddress, command.AmountInEth);
        }

        string? fundWithTruTxnHash = null;
        if (command.AmountInTru > 0)
        {
            fundWithTruTxnHash = await _contractCaller.MintAndDepositTruthserumTo(
                command.WalletAddress, new BigInteger(command.AmountInTru * 1000000000)
            );
        }

        return new()
        {
            Data = new()
            {
                EthTxnHash = fundWithEthTxnHash,
                TruTxnHash = fundWithTruTxnHash
            }
        };
    }
}
